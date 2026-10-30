
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qms.Api.Data;
using Qms.Api.Models;
using Qms.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Application services
builder.Services.AddScoped<MasterListExcelPreviewService>();
builder.Services.AddScoped<MasterListService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddScoped<ISigninService, SigninService>();
builder.Services.AddScoped<ISignupService, SignupService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IMasterListImportService, MasterListImportService>();

builder.Services.AddScoped<MasterListExcelPreviewService>();
builder.Services.AddScoped<MasterListService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
// Controllers
builder.Services.AddControllers();

// Database
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<QmsDbContext>(options =>
    options.UseSqlServer(connectionString));

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing.");

byte[] keyBytes;

try
{
  keyBytes = Convert.FromBase64String(jwtKey);
}
catch (FormatException ex)
{
  throw new InvalidOperationException(
      "Jwt:Key must be a Base64-encoded secret.", ex);
}

if (keyBytes.Length < 32)
{
  throw new InvalidOperationException(
      "Jwt:Key must contain at least 32 random bytes.");
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services
    .AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme =
          JwtBearerDefaults.AuthenticationScheme;

      options.DefaultChallengeScheme =
          JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
      };
    });

builder.Services.AddAuthorization();

// Angular CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("QmsAngularPolicy", policy =>
  {
    policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod();
  });
});

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger is enabled in Development
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("QmsAngularPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
