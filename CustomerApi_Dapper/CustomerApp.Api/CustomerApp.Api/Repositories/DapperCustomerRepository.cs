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