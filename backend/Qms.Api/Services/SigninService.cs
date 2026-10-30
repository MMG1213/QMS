using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class SigninService : ISigninService
{
    private readonly QmsDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _configuration;

    public SigninService(
        QmsDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginServiceResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LoginId) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginServiceResult.Failure(
                "Invalid credentials or inactive account.");
        }

        var loginId = request.LoginId.Trim();

        var user = await _context.AppUsers
            .Include(u => u.Department)
            .SingleOrDefaultAsync(u =>
                u.EmployeeId == loginId ||
                u.Email == loginId);

        if (user == null ||
            !user.IsActive ||
            string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return LoginServiceResult.Failure(
                "Invalid credentials or inactive account.");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return LoginServiceResult.Failure(
                "Invalid credentials or inactive account.");
        }

        var validRoles = new[]
        {
            QmsRole.User,
            QmsRole.DepartmentHead,
            QmsRole.QmsAdmin
        };

        if (!validRoles.Contains(user.Role, StringComparer.Ordinal))
        {
            return LoginServiceResult.Failure(
                "This account has an invalid role configuration.");
        }

        if (user.Role != QmsRole.QmsAdmin &&
            user.DepartmentId == null)
        {
            return LoginServiceResult.Failure(
                "This account has no department assigned.");
        }

        if (passwordResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
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
            return LoginServiceResult.ConfigurationError(
                "JWT configuration is incomplete.");
        }

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(tokenKey);
        }
        catch (FormatException)
        {
            return LoginServiceResult.ConfigurationError(
                "JWT signing key configuration is invalid.");
        }

        var key = new SymmetricSecurityKey(keyBytes);

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

        var response = new LoginResponse
        {
            Token = tokenString,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            EmployeeId = user.EmployeeId,
            FullName = user.FullName,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            Role = user.Role
        };

        return LoginServiceResult.Success(response);
    }
}
