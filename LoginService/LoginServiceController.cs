using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Microservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/login")]
public class LoginServiceController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ITokenValidationService _tokenValidationService;

    public LoginServiceController(IUserService userService, ITokenService tokenService, ITokenValidationService tokenvalidationService)
    {
        _userService = userService;
        _tokenService = tokenService;   
        _tokenValidationService=tokenvalidationService;    
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.AuthenticateAsync(request.Id, request.UserName);

        if (user == null)
            return Unauthorized();

        var token = await _tokenService.GenerateToken(user);
        return Ok(new { Token = token });
    }
    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] string token)
    {
        // Add your token validation logic here
        // Example: check if token is valid or expired

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid token");
        }

        // Your token validation logic...

        if (_tokenValidationService.ValidateToken(token))
        {
            return Ok();
        }
        else
        {
            return Unauthorized();
        }
    }
}


public class LoginRequest
{
    public int Id { get; set; }
    public string? UserName { get; set; }
}

public interface ITokenService
{
    public Task<string> GenerateToken(LoginRequest request);
}

public interface IUserService
{
    public Task<LoginRequest?> AuthenticateAsync(int Id, string? userName);
}

public class UserService : IUserService
{
     private readonly UserRepository _userRepository;

     public UserService(UserRepository userRepository)
     {
            _userRepository = userRepository;
     }
    public Task<LoginRequest?> AuthenticateAsync(int Id, string? UserName)
    {
        var result =_userRepository.GetCustomerById(Id);
        if (result == null)
            return Task.FromResult<LoginRequest?>(null);
        return Task.FromResult<LoginRequest?>(new LoginRequest { Id = result.Id, UserName = result.Name });
    }
}

public class TokenService : ITokenService
{
    public Task<string> GenerateToken(LoginRequest request)
    {
         
        // Define your secret key (you should keep this secure)
        string secretKey = "DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI=";

        // Convert the secret key to bytes
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        // Create signing credentials using the secret key and HMAC-SHA256 algorithm
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        // Create claims for the JWT token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, request.UserName ?? string.Empty) // Fix: Pass the claim type and value as strings
            // Add additional claims as needed
        };

        // Create a JWT security token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
            SigningCredentials = signingCredentials
        };

        // Create a JWT security token handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Generate the JWT token
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        // Encode the JWT token to its string representation
        var encodedToken = tokenHandler.WriteToken(token);

        return Task.FromResult(encodedToken);
    
    }
}