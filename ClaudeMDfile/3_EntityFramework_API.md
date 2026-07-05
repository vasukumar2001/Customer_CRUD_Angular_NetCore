# Customer API — Entity Framework (Repository Pattern + Unit of Work)

This version wraps EF Core behind a **Repository** (`ICustomerRepository`) and a **Unit of Work** (`IUnitOfWork`). The controller only ever talks to the `IUnitOfWork` **interface** — it never sees `DbContext` or the concrete repository class directly. That's the standard clean layering: Controller → IUnitOfWork → ICustomerRepository → DbContext.

## Step 1: Database table

```sql
CREATE TABLE Customer (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    EmailId VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    MobilePhone VARCHAR(20) NULL,
    HomePhone VARCHAR(20) NULL,
    Address VARCHAR(250) NULL
);
```

## Step 2: Install EF packages

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

## Step 3: appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Step 4: Folder structure

```
CustomerApp.Api/
 ├── Models/
 │    └── Customer.cs
 ├── Validations/
 │    └── ValidEmailAttribute.cs
 ├── Data/
 │    └── CustomerDbContext.cs
 ├── Repositories/
 │    ├── ICustomerRepository.cs
 │    └── CustomerRepository.cs
 ├── UnitOfWork/
 │    ├── IUnitOfWork.cs
 │    └── UnitOfWork.cs
 ├── Controllers/
 │    └── CustomersController.cs
 └── Program.cs
```

## Step 5: Model — `Models/Customer.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using CustomerApp.Api.Validations;

namespace CustomerApp.Api.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [ValidEmail(ErrorMessage = "Email is not valid")]
        public string EmailId { get; set; }

        public string? PhoneNumber { get; set; }
        public string? MobilePhone { get; set; }
        public string? HomePhone { get; set; }
        public string? Address { get; set; }

        // Not stored in DB — computed for the grid only
        public string ContactNumber =>
            !string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber :
            !string.IsNullOrEmpty(MobilePhone) ? MobilePhone :
            HomePhone;
    }
}
```

## Step 6: Custom email validation — `Validations/ValidEmailAttribute.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CustomerApp.Api.Validations
{
    public class ValidEmailAttribute : ValidationAttribute
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null) return ValidationResult.Success;

            string email = value.ToString();
            if (!EmailRegex.IsMatch(email))
                return new ValidationResult("Email format is invalid");

            return ValidationResult.Success;
        }
    }
}
```

## Step 7: DbContext — `Data/CustomerDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using CustomerApp.Api.Models;

namespace CustomerApp.Api.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");
                entity.HasKey(c => c.CustomerId);
                entity.Ignore(c => c.ContactNumber); // computed property, not a DB column
            });
        }
    }
}
```

## Step 8: Repository interface — `Repositories/ICustomerRepository.cs`

The repository only deals with `Customer` objects in memory. Note there's **no `SaveChanges` here** — saving is the Unit of Work's job, not the repository's.

```csharp
using CustomerApp.Api.Models;

namespace CustomerApp.Api.Repositories
{
    public interface ICustomerRepository
    {
        List<Customer> GetAll();
        Customer GetById(int id);
        void Add(Customer customer);
        void Update(Customer customer);
        void Delete(Customer customer);
    }
}
```

## Step 9: Repository implementation — `Repositories/CustomerRepository.cs`

```csharp
using CustomerApp.Api.Data;
using CustomerApp.Api.Models;

namespace CustomerApp.Api.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDbContext _context;

        public CustomerRepository(CustomerDbContext context)
        {
            _context = context;
        }

        public List<Customer> GetAll() => _context.Customers.ToList();

        public Customer GetById(int id) => _context.Customers.Find(id);

        public void Add(Customer customer) => _context.Customers.Add(customer);

        public void Update(Customer customer) => _context.Customers.Update(customer);

        public void Delete(Customer customer) => _context.Customers.Remove(customer);
    }
}
```

## Step 10: Unit of Work interface — `UnitOfWork/IUnitOfWork.cs`

This is what the controller will actually depend on. It exposes the repository and one `Complete()` method that commits everything in a single `SaveChanges()` call.

```csharp
using CustomerApp.Api.Repositories;

namespace CustomerApp.Api.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        int Complete();
    }
}
```

## Step 11: Unit of Work implementation — `UnitOfWork/UnitOfWork.cs`

```csharp
using CustomerApp.Api.Data;
using CustomerApp.Api.Repositories;

namespace CustomerApp.Api.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly CustomerDbContext _context;

        public ICustomerRepository Customers { get; }

        public UnitOfWork(CustomerDbContext context)
        {
            _context = context;
            Customers = new CustomerRepository(_context);
        }

        public int Complete() => _context.SaveChanges();

        public void Dispose() => _context.Dispose();
    }
}
```

## Step 12: Controller — `Controllers/CustomersController.cs`

The controller injects **`IUnitOfWork` only** — it never sees `CustomerDbContext` or `CustomerRepository` directly.

```csharp
using Microsoft.AspNetCore.Mvc;
using CustomerApp.Api.Models;
using CustomerApp.Api.UnitOfWork;

namespace CustomerApp.Api.Controllers
{
    [ApiController]
    [Route("api/ef/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_unitOfWork.Customers.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var customer = _unitOfWork.Customers.GetById(id);
            return customer == null ? NotFound() : Ok(customer);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Customer customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _unitOfWork.Customers.Add(customer);
            _unitOfWork.Complete();

            return Ok(new { customer.CustomerId });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Customer customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = _unitOfWork.Customers.GetById(id);
            if (existing == null) return NotFound();

            existing.Name = customer.Name;
            existing.EmailId = customer.EmailId;
            existing.PhoneNumber = customer.PhoneNumber;
            existing.MobilePhone = customer.MobilePhone;
            existing.HomePhone = customer.HomePhone;
            existing.Address = customer.Address;

            _unitOfWork.Customers.Update(existing);
            _unitOfWork.Complete();

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _unitOfWork.Customers.GetById(id);
            if (existing == null) return NotFound();

            _unitOfWork.Customers.Delete(existing);
            _unitOfWork.Complete();

            return Ok();
        }
    }
}
```

## Step 13: Register in DI — `Program.cs`

Register the interfaces, not the concrete classes — so the controller only ever knows about `IUnitOfWork`.

```csharp
using Microsoft.EntityFrameworkCore;
using CustomerApp.Api.Data;
using CustomerApp.Api.Repositories;
using CustomerApp.Api.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUnitOfWork, CustomerApp.Api.UnitOfWork.UnitOfWork>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();
app.Run();
```

## Step 14 (optional): Let EF create the table via migrations instead of Step 1's script

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Step 15: Run and test

```bash
dotnet run
```

Open Swagger and test `GET/POST/PUT/DELETE /api/ef/customers`.

---

## Why this layering

| Layer | Depends on | Purpose |
|---|---|---|
| `CustomersController` | `IUnitOfWork` (interface only) | Handles HTTP, never touches EF directly |
| `UnitOfWork` | `ICustomerRepository`, `CustomerDbContext` | Coordinates repositories, commits one `SaveChanges()` per request |
| `CustomerRepository` | `CustomerDbContext` | Only place that queries/mutates `Customer` data |
| `CustomerDbContext` | EF Core | Actual database access |

If you later add more entities (e.g. `Order`, `Item`), you just add more repository properties to `IUnitOfWork` (e.g. `Orders`, `Items`) — the controller still only depends on the one `IUnitOfWork` interface, and `Complete()` still commits everything together in a single transaction.
