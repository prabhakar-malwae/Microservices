using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microservice;
using System.Security.Cryptography;
var builder = WebApplication.CreateBuilder(args);

var key = "DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI="; // This should be a secret key stored in a secure location
var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
        ValidateIssuer = true, // Enable issuer validation
        ValidateAudience = true, // Enable audience validation
        ValidIssuer = "http://localhost:5014/", // Specify the expected issuer
        ValidAudiences = new[] { "customerservice", "customerhistory" } // Specify the expected audience
    };
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString != null)
{
    builder.Services.AddScoped(_ => new UserRepository(connectionString));
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

// Register the controllers in Swagger
app.MapControllers();

app.Run();

app.Run();
