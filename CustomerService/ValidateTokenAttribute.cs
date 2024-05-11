using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
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
        var response = await _httpClient.GetAsync($"https://loginservice-url/api/auth/validate?token={token}");
        return response.IsSuccessStatusCode;
    }
}
