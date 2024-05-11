using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/login")]
public class LoginServiceController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public LoginServiceController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
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
    public Task<LoginRequest> AuthenticateAsync(int Id, string? userName);
}

public class UserService : IUserService
{
    public Task<LoginRequest> AuthenticateAsync(int Id, string? UserName)
    {
        throw new NotImplementedException();
    }
}

public class TokenService : ITokenService
{
    public Task<string> GenerateToken(LoginRequest request)
    {
        throw new NotImplementedException();
    }
}