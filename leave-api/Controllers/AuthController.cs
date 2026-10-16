using LeaveApi.Data;
using LeaveApi.Dtos;
using LeaveApi.Models;
using LeaveApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private static readonly string[] Roles = ["Admin", "Manager", "Employee"];
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username and password are required.");

        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Employee" : dto.Role;
        if (!Roles.Contains(role))
            return BadRequest($"Role must be one of: {string.Join(", ", Roles)}.");

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return Conflict("Username already exists.");

        var salt = PasswordHasher.GenerateSalt();
        var user = new User
        {
            Username = dto.Username,
            Salt = salt,
            PasswordHash = PasswordHasher.Hash(dto.Password, salt),
            Role = role,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.LeaveBalances.Add(new LeaveBalance
        {
            UserId = user.Id,
            Year = DateTime.UtcNow.Year,
            TotalDays = LeaveService.DefaultYearlyAllowance,
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Register),
            new { username = user.Username, role = user.Role });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null || !PasswordHasher.Verify(dto.Password, user.Salt, user.PasswordHash))
            return Unauthorized("Invalid username or password.");

        return new AuthResponse(_jwt.GenerateToken(user), user.Username, user.Role);
    }
}
