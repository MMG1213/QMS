
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly QmsDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        QmsDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var loginId = request.LoginId.Trim();

        var user = await _context.AppUsers
            .Include(u => u.Department)
            .SingleOrDefaultAsync(u =>
                u.EmployeeId == loginId ||
                u.Email == loginId);

        // Do not allow inactive accounts or accounts without initialized
        // password hashes to authenticate.
        if (user == null ||
            !user.IsActive ||
            string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Unauthorized(new
            {
                message = "Invalid credentials or inactive account."
            });
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials or inactive account."
            });
        }

        // Reject unknown roles rather than issuing an unexpected role claim.
        var validRoles = new[]
        {
            QmsRole.User,
            QmsRole.DepartmentHead,
            QmsRole.QmsAdmin
        };

        if (!validRoles.Contains(user.Role, StringComparer.Ordinal))
        {
            return Unauthorized(new
            {
                message = "This account has an invalid role configuration."
            });
        }

        // All non-admin accounts must belong to a department.
        if (user.Role != QmsRole.QmsAdmin &&
            user.DepartmentId == null)
        {
            return Unauthorized(new
            {
                message = "This account has no department assigned."
            });
        }

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            user.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        var tokenKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(tokenKey) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT configuration is incomplete.");
        }

        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(tokenKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.EmployeeId),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("employeeId", user.EmployeeId)
        };

        if (user.DepartmentId.HasValue)
        {
            claims.Add(new Claim(
                "departmentId",
                user.DepartmentId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new LoginResponse
        {
            Token = tokenString,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            EmployeeId = user.EmployeeId,
            FullName = user.FullName,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            Role = user.Role
        });
    }
}
