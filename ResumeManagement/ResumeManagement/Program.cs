using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResumeManagement.Application.Interfaces;
using ResumeManagement.Application.Services;
using ResumeManagement.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("OracleConnection");

builder.Services.AddDbContext<ResumeDbContext>(options =>
    options.UseOracle(connectionString));

builder.Services.AddScoped<ResumeService>();
builder.Services.AddScoped<ILogService, LogService>();


builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost: 6379"; 
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "ResumeManagementSystem",
            ValidAudience = "ResumeManagementUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

           /* new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("SuperSecretKeyThatIsLongEnoughForHS256AlgorithmWhichIsAtLeast32CharactersLong"))*/
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("HRPolicy", policy => policy.RequireRole("HR"));
    options.AddPolicy("RecruiterPolicy", policy => policy.RequireRole("Recruiter"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
