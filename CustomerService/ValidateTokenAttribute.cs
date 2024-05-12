using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class ValidateTokenAttribute : ActionFilterAttribute
{
    private readonly HttpClient _httpClient;

    public ValidateTokenAttribute(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.ContainsKey("Authorization"))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var isValid = await ValidateTokenWithLoginService(token);

        if (!isValid)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }

   private async Task<bool> ValidateTokenWithLoginService(string token)
{
    // Create an anonymous object with the token
    TokenValidationRequest tokenObject = new TokenValidationRequest();
    tokenObject.Token = token;

    // Send a POST request with the token as JSON
    var response = await _httpClient.PostAsJsonAsync("http://localhost:5014/api/login/validate-token", tokenObject);

    // Check the response status code
    if (!response.IsSuccessStatusCode)
    {
        // Read the response content for more details on the error
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {errorContent}");
        return false;
    }

    return true;
}

public class TokenValidationRequest
{
    public string Token { get; set; }
}

}
