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