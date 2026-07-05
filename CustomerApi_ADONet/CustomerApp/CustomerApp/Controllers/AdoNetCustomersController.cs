
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