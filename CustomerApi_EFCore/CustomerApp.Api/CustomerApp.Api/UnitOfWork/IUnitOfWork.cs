using CustomerApp.Api.Repositories;

namespace CustomerApp.Api.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        int Complete();
    }
}