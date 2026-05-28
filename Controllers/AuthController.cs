using coreWebApi.Dtos;
using coreWebApi.Helpers;
using coreWebApi.Models;
using coreWebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    public AuthController(
        IConfiguration config,
        AppDbContext context,
        IJwtService jwtService)
    {
        _config = config;
        _context = context;
        _jwtService = jwtService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok("Admin access");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var emailExists = await _context.Users
        .AnyAsync(x => x.Email == dto.Email);

        if (emailExists)
            return BadRequest(new { message = "Email already exists" });

        //var userNameExists = await _context.Users
        //.AnyAsync(x => x.Username == dto.Username);

        //if (userNameExists)
        //    return BadRequest(new { message = "UserName already exists" });

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            Gender = dto.Gender,
            PasswordHash = PasswordHelper.Hash(dto.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Register successfully",
            user.Id,
            user.FullName,
            user.Email
        });
    }

    //[Authorize(Roles = "Admin")]
    [HttpPost("adminRegister")]
    public async Task<IActionResult> AdminRegister([FromBody] AdminRegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var emailExists = await _context.Users
        .AnyAsync(x => x.Email == dto.Email);

        if (emailExists)
            return BadRequest(new { message = "Email already exists" });
        var user = new User
        {
            Email = dto.Email,
            Gender = dto.Gender,
            FullName = dto.FullName,
            PasswordHash = PasswordHelper.Hash(dto.Password),
            Role = "Admin"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Register admin successfully",
            user.Id,
            user.FullName,
            user.Email
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginDto dto)
    {
        var hash = PasswordHelper.Hash(dto.Password);

        var user = _context.Users.FirstOrDefault(x =>
            x.Email == dto.Email && x.PasswordHash == hash);

        if (user == null)
            return Unauthorized();

        var accessToken = _jwtService.CreateAccessToken(user, _config);

        var refreshToken = new RefreshToken
        {

            Token = new JwtHelper().GenerateRefreshToken(),
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Login successfully",
            accessToken,
            refreshToken = refreshToken.Token,
            user = new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Gender,
                user.Role,
                user.CreatedAt,
                user.UpdatedAt
            }
        });
    }

    [Authorize]
    [HttpPost("refresh")]
    public IActionResult Refresh(string refreshToken)
    {
        var token = _context.RefreshTokens
            .FirstOrDefault(x => x.Token == refreshToken && !x.IsRevoked);

        if (token == null || token.ExpiryDate < DateTime.UtcNow)
            return Unauthorized();

        var user = _context.Users.Find(token.UserId);

        var newAccessToken = _jwtService.CreateAccessToken(user, _config);

        return Ok(new
        {
            accessToken = newAccessToken
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var userIdInt = int.Parse(userId);

        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userIdInt && !x.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }

}