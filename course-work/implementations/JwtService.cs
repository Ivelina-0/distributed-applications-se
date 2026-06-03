using DentalClinicApp.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DentalClinicApp.Services;

public interface IJwtService { string GenerateToken(User user); }

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config) => _config = config;
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"]!));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: expires, signingCredentials: creds));
    }
}
