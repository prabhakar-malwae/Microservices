// LoginController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Microservice
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {

        private readonly CustomerRepository _customerRepository;
        public  LoginController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] LoginRequest model)
        {
            var customer = _customerRepository.GetCustomerById(model.Id);

            if(customer != null)
            { // Assume user ID is validated successfully
                var token = GenerateJwtToken(model.UserId);

                return Ok(new { Token = token });
            }
            else{
                return Unauthorized();
            }
        }

        private string GenerateJwtToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI=");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                    // Add additional claims as needed
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public required string UserId { get; set; }
        public required int Id {get;set;}
        // You can add other properties like password if needed
    }
}
