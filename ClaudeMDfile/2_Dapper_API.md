# Customer API — Dapper (Stored Procedures)

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

## Step 2: Stored procedures

```sql
CREATE PROCEDURE sp_Customer_GetAll
AS
BEGIN
    SELECT CustomerId, Name, EmailId, PhoneNumber, MobilePhone, HomePhone, Address
    FROM Customer
END
GO

CREATE PROCEDURE sp_Customer_GetById
    @CustomerId INT
AS
BEGIN
    SELECT CustomerId, Name, EmailId, PhoneNumber, MobilePhone, HomePhone, Address
    FROM Customer
    WHERE CustomerId = @CustomerId
END
GO

CREATE PROCEDURE sp_Customer_Add
    @Name VARCHAR(100),
    @EmailId VARCHAR(100),
    @PhoneNumber VARCHAR(20) = NULL,
    @MobilePhone VARCHAR(20) = NULL,
    @HomePhone VARCHAR(20) = NULL,
    @Address VARCHAR(250) = NULL
AS
BEGIN
    INSERT INTO Customer (Name, EmailId, PhoneNumber, MobilePhone, HomePhone, Address)
    VALUES (@Name, @EmailId, @PhoneNumber, @MobilePhone, @HomePhone, @Address)

    SELECT CAST(SCOPE_IDENTITY() AS INT)
END
GO

CREATE PROCEDURE sp_Customer_Update
    @CustomerId INT,
    @Name VARCHAR(100),
    @EmailId VARCHAR(100),
    @PhoneNumber VARCHAR(20) = NULL,
    @MobilePhone VARCHAR(20) = NULL,
    @HomePhone VARCHAR(20) = NULL,
    @Address VARCHAR(250) = NULL
AS
BEGIN
    UPDATE Customer
    SET Name = @Name,
        EmailId = @EmailId,
        PhoneNumber = @PhoneNumber,
        MobilePhone = @MobilePhone,
        HomePhone = @HomePhone,
        Address = @Address
    WHERE CustomerId = @CustomerId
END
GO

CREATE PROCEDURE sp_Customer_Delete
    @CustomerId INT
AS
BEGIN
    DELETE FROM Customer WHERE CustomerId = @CustomerId
END
GO
```

## Step 3: Install Dapper

```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
dotnet add package Swashbuckle.AspNetCore
```

## Step 4: appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Step 5: Model — `Models/Customer.cs`

(Same model used across all three approaches.)

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

## Step 7: Repository interface — `Repositories/ICustomerRepository.cs`

```csharp
using CustomerApp.Api.Models;

namespace CustomerApp.Api.Repositories
{
    public interface ICustomerRepository
    {
        List<Customer> GetAll();
        Customer GetById(int id);
        int Add(Customer customer);
        bool Update(Customer customer);
        bool Delete(int id);
    }
}
```

## Step 8: Dapper repository — `Repositories/DapperCustomerRepository.cs`

Calls stored procedures only — no SQL text hardcoded anywhere.

```csharp
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using CustomerApp.Api.Models;

namespace CustomerApp.Api.Repositories
{
    public class DapperCustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public DapperCustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection GetConnection() => new SqlConnection(_connectionString);

        public List<Customer> GetAll()
        {
            using var conn = GetConnection();
            return conn.Query<Customer>(
                "sp_Customer_GetAll",
                commandType: CommandType.StoredProcedure).ToList();
        }

        public Customer GetById(int id)
        {
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<Customer>(
                "sp_Customer_GetById",
                new { CustomerId = id },
                commandType: CommandType.StoredProcedure);
        }

        public int Add(Customer c)
        {
            using var conn = GetConnection();
            return conn.ExecuteScalar<int>(
                "sp_Customer_Add",
                new
                {
                    c.Name,
                    c.EmailId,
                    c.PhoneNumber,
                    c.MobilePhone,
                    c.HomePhone,
                    c.Address
                },
                commandType: CommandType.StoredProcedure);
        }

        public bool Update(Customer c)
        {
            using var conn = GetConnection();
            var rows = conn.Execute(
                "sp_Customer_Update",
                new
                {
                    c.CustomerId,
                    c.Name,
                    c.EmailId,
                    c.PhoneNumber,
                    c.MobilePhone,
                    c.HomePhone,
                    c.Address
                },
                commandType: CommandType.StoredProcedure);
            return rows > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            var rows = conn.Execute(
                "sp_Customer_Delete",
                new { CustomerId = id },
                commandType: CommandType.StoredProcedure);
            return rows > 0;
        }
    }
}
```

> Note: Dapper leaves `Customer.ContactNumber` as `null` since it's a computed C# property with no matching DB column — that's expected, the JSON response still includes the property fine.

## Step 9: Controller — `Controllers/DapperCustomersController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using CustomerApp.Api.Models;
using CustomerApp.Api.Repositories;

namespace CustomerApp.Api.Controllers
{
    [ApiController]
    [Route("api/dapper/customers")]
    public class DapperCustomersController : ControllerBase
    {
        private readonly DapperCustomerRepository _repository;

        public DapperCustomersController(DapperCustomerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repository.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var c = _repository.GetById(id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Customer customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = _repository.Add(customer);
            return Ok(new { CustomerId = id });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Customer customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            customer.CustomerId = id;
            return _repository.Update(customer) ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => _repository.Delete(id) ? Ok() : NotFound();
    }
}
```

## Step 10: Register in DI — `Program.cs`

```csharp
using CustomerApp.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DapperCustomerRepository>();

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

## Step 11: Run and test

```bash
dotnet run
```

Open Swagger and test `GET/POST/PUT/DELETE /api/dapper/customers`.
