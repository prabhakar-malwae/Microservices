using Microservice;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

var key = Encoding.ASCII.GetBytes("DbFCH///fLPOk4Yaw8hvmFueG3y4yvb8KW+jZaLMDOI="); // This should be a secret key stored in a secure location

 // Configure JWT authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "http://localhost:5014/"; // URL of the LoginService
                    options.Audience = "customerservice"; // Expected audience value
                    options.RequireHttpsMetadata = false;
                });
// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ValidateTokenAttribute));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString != null)
{
    builder.Services.AddScoped(_ => new CustomerRepository(connectionString));
    builder.Services.AddHttpClient();
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
