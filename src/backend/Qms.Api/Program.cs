
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qms.Api.Data;
using Qms.Api.Models;
using Qms.Api.Services;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

// Application services
builder.Services.AddScoped<MasterListExcelPreviewService>();
builder.Services.AddScoped<MasterListService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<ISigninService, SigninService>();
builder.Services.AddScoped<ISignupService, SignupService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IMasterListImportService, MasterListImportService>();
builder.Services.AddScoped<ML01Service>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<MasterListExcelExportService>();
builder.Services.AddScoped<IAdminSignupRequestService, AdminSignupRequestService>();
builder.Services.AddScoped<IDocumentTypeAdminService, DocumentTypeAdminService>();
builder.Services.AddScoped<IDocumentApprovalMatrixService, DocumentApprovalMatrixService>();

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

        // Validate account status and token version on every authenticated request.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var tokenVersionClaim = context.Principal?
                    .FindFirst("tokenVersion")?.Value;

                if (!int.TryParse(userIdClaim, out var userId) ||
                    !int.TryParse(tokenVersionClaim, out var tokenVersion))
                {
                    context.Fail("Invalid authentication token.");
                    return;
                }

                var db = context.HttpContext.RequestServices
                    .GetRequiredService<QmsDbContext>();

                var accountIsValid = await db.AppUsers
                    .AsNoTracking()
                    .AnyAsync(user =>
                        user.Id == userId &&
                        user.IsActive &&
                        user.TokenVersion == tokenVersion &&
                        (
                            user.Role == QmsRole.QmsAdmin ||
                            (
                                user.DepartmentId.HasValue &&
                                db.QMSDepartments.Any(department =>
                                    department.DepartmentId == user.DepartmentId.Value &&
                                    department.IsActive)
                            )
                        ),
                        context.HttpContext.RequestAborted);

                if (!accountIsValid)
                {
                    context.Fail(
                        "Account is inactive, department is restricted, or token has been revoked.");
                }
            }
        };
    });

// Authorization
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
else
{
    // Local dev runs on plain HTTP (http launch profile) behind the Angular proxy,
    // so there is no HTTPS port to redirect to there.
    app.UseHttpsRedirection();
}

app.UseCors("QmsAngularPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

foreach (var endpoint in app.Services
    .GetRequiredService<EndpointDataSource>()
    .Endpoints)
{
    Console.WriteLine(endpoint.DisplayName);
}

app.Run();
