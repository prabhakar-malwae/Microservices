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
        _tokenValidationService = tokenvalidationService;
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

        try
        {

            var claims_principal = _tokenService.ValidateToken(token);
            if (claims_principal.Identity.IsAuthenticated == false)
            {
                return Unauthorized();
            }
            else
            {
                return Ok(true);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
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
    public ClaimsPrincipal ValidateToken(string token);

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
        var result = _userRepository.GetCustomerById(Id);
        if (result == null)
            return Task.FromResult<LoginRequest?>(null);
        return Task.FromResult<LoginRequest?>(new LoginRequest { Id = result.Id, UserName = result.Name });
    }
}

public class TokenService : ITokenService
{
    private const string SecretKey = "DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI=";
    private const string KeyId = "kid"; // Replace with your actual key ID
    private const string Issuer = "http://localhost:5014";
    private const string Audience = "customerservice";

    public Task<string> GenerateToken(LoginRequest request)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var header = new JwtHeader(credentials)
        {
            { "kid", KeyId }
        };

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, request.Id.ToString()),
            new Claim(ClaimTypes.Name, request.UserName ?? "")
        };

        var payload = new JwtPayload(Issuer, Audience, claims, null, DateTime.UtcNow.AddHours(1));
        var token = new JwtSecurityToken(header, payload);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    // Assume this method exists and retrieves the correct signing key for a given 'kid'
    private SecurityKey GetSigningKey(string kid)
    {
        // Logic to retrieve the key from a secure location (e.g., database, key vault)
        // For example, this could be a lookup in a dictionary, a database query, etc.
        // This is a placeholder for demonstration purposes
        var keyDictionary = new Dictionary<string, string>
        {
            { "kid", "DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI=" }
        };

        if (keyDictionary.TryGetValue(kid, out var key))
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        throw new InvalidOperationException("Key ID not found.");
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI="));
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5014",
            ValidateAudience = true,
            ValidAudience = "customerservice",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        return tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
    }

}