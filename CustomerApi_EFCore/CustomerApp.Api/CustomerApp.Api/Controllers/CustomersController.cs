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