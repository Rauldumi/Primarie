using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Primarie.Data;
using Primarie.Dtos;
using Primarie.Entities;

namespace Primarie.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtOptions _jwt;

    public AuthService(AppDbContext db, IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
            throw new InvalidOperationException("Email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            Role = request.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwt(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = user.Role,
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            throw new InvalidOperationException("Invalid credentials.");

        var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!ok)
            throw new InvalidOperationException("Invalid credentials.");

        var token = GenerateJwt(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = user.Role,
            Token = token
        };
    }

    private string GenerateJwt(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var keyBytes = Encoding.UTF8.GetBytes(_jwt.Key);
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
