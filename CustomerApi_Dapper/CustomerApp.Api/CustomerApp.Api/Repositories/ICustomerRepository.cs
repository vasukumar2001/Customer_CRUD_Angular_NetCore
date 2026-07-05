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