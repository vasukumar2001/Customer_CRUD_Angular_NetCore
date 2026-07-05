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