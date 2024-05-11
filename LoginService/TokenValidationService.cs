using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


public interface ITokenValidationService
{
    public bool ValidateToken(string token);
}
public class TokenValidationService : ITokenValidationService
{
    private readonly string _secretKey; // Secret key used for token signing

    public TokenValidationService()
    {
        _secretKey ="DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI=";
    }

    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_secretKey)), // Convert secret key from base64
            ValidateIssuer = false, // You can set these to true if you have issuer and audience validation requirements
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ValidateTokenReplay = false
        };

        try
        {
            // Validate token
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true; // Token is valid
        }
        catch (Exception ex)
        {
            return false; // Token validation failed
        }
    }
}
