using DentalClinicApp.Data;
using DentalClinicApp.DTOs;
using DentalClinicApp.Models;
using DentalClinicApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IJwtService _jwt; private readonly PasswordHasher<User> _hasher = new();
    public AuthController(AppDbContext db, IJwtService jwt) { _db = db; _jwt = jwt; }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest r)
    {
        if (await _db.Users.AnyAsync(x => x.Email == r.Email)) return Conflict(new ErrorResponse(409, "Email already exists"));
        var user = new User { FullName = r.FullName, Email = r.Email, PhoneNumber = r.PhoneNumber, Role = r.Role };
        user.PasswordHash = _hasher.HashPassword(user, r.Password);
        _db.Users.Add(user);
        if (r.Role == UserRole.Patient && !await _db.Patients.AnyAsync(x => x.Email == r.Email))
            _db.Patients.Add(new Patient { FullName = r.FullName, Email = r.Email, PhoneNumber = r.PhoneNumber });
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Register), new UserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest r)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == r.Email && x.IsActive);
        if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, r.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new ErrorResponse(401, "Invalid email or password"));
        return Ok(new LoginResponse(_jwt.GenerateToken(user), user.Id, user.FullName, user.Role));
    }
}
