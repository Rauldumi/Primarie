using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Primarie.Data;
using Primarie.Dtos;
using Primarie.Services;
using System.Security.Claims;

namespace Primarie.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly AppDbContext _db;

    public AuthController(IAuthService auth, AppDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            var result = await _auth.RegisterAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            var result = await _auth.LoginAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        if (userIdStr is null)
            return Unauthorized();

        var userId = Guid.Parse(userIdStr);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized();

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            Role = user.Role.ToString()
        });
    }
}
