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