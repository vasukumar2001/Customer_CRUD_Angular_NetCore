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