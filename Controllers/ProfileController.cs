using coreWebApi.Helpers;
using coreWebApi.Services.Interfaces;
using DemoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Profile()
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Gender,
            user.Role,
            user.CreatedAt
        });

    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null)
            return NotFound();

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Gender = dto.Gender;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = PasswordHelper.Hash(dto.Password);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Update profile successfully",
            user.Id,
            user.FullName,
            user.Email,
            user.Gender
        });
    }
    //[Authorize]
    //[HttpPost("buy/{productId}")]
    //public async Task<IActionResult> AddProductToProfile(int productId)
    //{
    //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //    var product = await _context.Products.FindAsync(productId);
    //    if (product == null)
    //        return NotFound(new { message = "Product not found" });

    //    var exists = await _context.OrderDetails
    //        .AnyAsync(x => x.UserId == userId && x.ProductId == productId);

    //    if (exists)
    //        return BadRequest(new { message = "Product already added" });

    //    var orderDetails = new OrderDetails
    //    {
    //        UserId = userId,
    //        ProductId = productId
    //    };

    //    _context.OrderDetails.Add(orderDetails);
    //    await _context.SaveChangesAsync();

    //    return Ok(new { message = "Product added to profile" });
    //}
}