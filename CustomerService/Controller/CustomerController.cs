using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservice
{
    [Authorize]
    [ApiController]
    [Route("/api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerRepository _customerRepository;
        public CustomerController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        [HttpGet]
        public IActionResult GetCustomers()
        {
            var customer = _customerRepository.GetAllCustomers();
            return Ok(customer);
        }
        [HttpGet]
        [Route("/api/customer/Id")]
        public IActionResult GetCustomerById(int Id)
        {
            var customer = _customerRepository.GetCustomerById(Id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }
        [HttpPost]
        public IActionResult Post([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool isInserted = _customerRepository.InsertCustomer(customer);
                if (isInserted)
                {
                    // Assuming you have a GetCustomerById action to fetch the customer by ID
                    return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
                }
                else
                {
                    return BadRequest("Could not insert the customer.");
                }
            }
            catch (Exception)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

    }
}
