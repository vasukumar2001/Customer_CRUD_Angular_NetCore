# Customer API — ADO.NET (Stored Procedures)

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

## Step 3: appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Install the SQL client package:
```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package Swashbuckle.AspNetCore
```

## Step 4: Model — `Models/Customer.cs`

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

## Step 5: Custom email validation — `Validations/ValidEmailAttribute.cs`

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
            if (value == null) return ValidationResult.Success; // [Required] already handles empty

            string email = value.ToString();
            if (!EmailRegex.IsMatch(email))
                return new ValidationResult("Email format is invalid");

            return ValidationResult.Success;
        }
    }
}
```

## Step 6: Repository interface — `Repositories/ICustomerRepository.cs`

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

## Step 7: ADO.NET repository — `Repositories/AdoNetCustomerRepository.cs`

Calls stored procedures only — no SQL text hardcoded anywhere.

```csharp
using System.Data;
using Microsoft.Data.SqlClient;
using CustomerApp.Api.Models;

namespace CustomerApp.Api.Repositories
{
    public class AdoNetCustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public AdoNetCustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public List<Customer> GetAll()
        {
            var list = new List<Customer>();
            using var conn = GetConnection();
            using var cmd = new SqlCommand("sp_Customer_GetAll", conn) { CommandType = CommandType.StoredProcedure };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public Customer GetById(int id)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("sp_Customer_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CustomerId", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public int Add(Customer c)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("sp_Customer_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddParams(cmd, c);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(Customer c)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("sp_Customer_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CustomerId", c.CustomerId);
            AddParams(cmd, c);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("sp_Customer_Delete", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CustomerId", id);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private void AddParams(SqlCommand cmd, Customer c)
        {
            cmd.Parameters.AddWithValue("@Name", c.Name);
            cmd.Parameters.AddWithValue("@EmailId", c.EmailId);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object)c.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobilePhone", (object)c.MobilePhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HomePhone", (object)c.HomePhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)c.Address ?? DBNull.Value);
        }

        private Customer Map(SqlDataReader reader) => new Customer
        {
            CustomerId = (int)reader["CustomerId"],
            Name = reader["Name"].ToString(),
            EmailId = reader["EmailId"].ToString(),
            PhoneNumber = reader["PhoneNumber"] as string,
            MobilePhone = reader["MobilePhone"] as string,
            HomePhone = reader["HomePhone"] as string,
            Address = reader["Address"] as string
        };
    }
}
```

## Step 8: Controller — `Controllers/AdoNetCustomersController.cs`

Written by hand — no scaffolding.

```csharp
using Microsoft.AspNetCore.Mvc;
using CustomerApp.Api.Models;
using CustomerApp.Api.Repositories;

namespace CustomerApp.Api.Controllers
{
    [ApiController]
    [Route("api/adonet/customers")]
    public class AdoNetCustomersController : ControllerBase
    {
        private readonly AdoNetCustomerRepository _repository;

        public AdoNetCustomersController(AdoNetCustomerRepository repository)
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

## Step 9: Register in DI — `Program.cs`

```csharp
using CustomerApp.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AdoNetCustomerRepository>();

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

## Step 10: Run and test

```bash
dotnet run
```

Open Swagger and test `GET/POST/PUT/DELETE /api/adonet/customers`.
