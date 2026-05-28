using coreWebApi.Dtos;
using coreWebApi.Helpers;
using coreWebApi.Models;
using coreWebApi.Services.Interfaces;
using DemoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly AppDbContext _context;

    public UsersController(IUserService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }


    //[HttpGet]
    //public async Task<IActionResult> GetAll([FromQuery] UserQueryDto dto)
    //{
    //    var usersQuery = _context.Users.AsQueryable();
    //    // Search theo tên hoặc email
    //    if (!string.IsNullOrWhiteSpace(dto.Keyword))
    //    {
    //        usersQuery = usersQuery.Where(x =>
    //        x.Email.Contains(dto.Keyword) ||
    //        x.FullName.Contains(dto.Keyword));
    //    }

    //    if (dto.Gender.HasValue &&
    //    !Enum.IsDefined(typeof(Gender), dto.Gender.Value))
    //    {
    //        return BadRequest("Gender không hợp lệ");
    //    }
    //    // 🚻 Filter theo Gender
    //    if (dto.Gender.HasValue)
    //    {
    //        usersQuery = usersQuery.Where(x => x.Gender == dto.Gender.Value);
    //    }
    //    // Sort
    //    usersQuery = dto.SortBy?.ToLower() switch
    //    {
    //        "username" => dto.SortOrder == "asc"
    //            ? usersQuery.OrderBy(x => x.FullName)
    //            : usersQuery.OrderByDescending(x => x.FullName),

    //        "email" => dto.SortOrder == "asc"
    //            ? usersQuery.OrderBy(x => x.Email)
    //            : usersQuery.OrderByDescending(x => x.Email),

    //        "gender" => dto.SortOrder == "asc"
    //            ? usersQuery.OrderBy(x => x.Gender)
    //            : usersQuery.OrderByDescending(x => x.Gender),

    //        _ => dto.SortOrder == "asc"
    //            ? usersQuery.OrderBy(x => x.Id)
    //            : usersQuery.OrderByDescending(x => x.Id)
    //    };

    //    var totalItems = await usersQuery.CountAsync();

    //    var users = await usersQuery
    //   .Skip((dto.Page - 1) * dto.PageSize)
    //   .Take(dto.PageSize)
    //   .ToListAsync();
    //    return Ok(new
    //    {
    //        page = dto.Page,
    //        pageSize = dto.PageSize,
    //        totalItems,
    //        totalPages = (int)Math.Ceiling(totalItems / (double)dto.PageSize),
    //        data = users
    //    });
    //}

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryDto dto)
    {
        var usersQuery = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Keyword))
        {
            usersQuery = usersQuery.Where(x =>
                x.Email.Contains(dto.Keyword) ||
                x.FullName.Contains(dto.Keyword));
        }

        if (dto.Gender.HasValue &&
            !Enum.IsDefined(typeof(Gender), dto.Gender.Value))
        {
            return BadRequest("Gender không hợp lệ");
        }

        if (dto.Gender.HasValue)
        {
            usersQuery = usersQuery.Where(x => x.Gender == dto.Gender.Value);
        }

        usersQuery = dto.SortBy?.ToLower() switch
        {
            "username" => dto.SortOrder == "asc"
                ? usersQuery.OrderBy(x => x.FullName)
                : usersQuery.OrderByDescending(x => x.FullName),

            "email" => dto.SortOrder == "asc"
                ? usersQuery.OrderBy(x => x.Email)
                : usersQuery.OrderByDescending(x => x.Email),

            "gender" => dto.SortOrder == "asc"
                ? usersQuery.OrderBy(x => x.Gender)
                : usersQuery.OrderByDescending(x => x.Gender),

            _ => dto.SortOrder == "asc"
                ? usersQuery.OrderBy(x => x.Id)
                : usersQuery.OrderByDescending(x => x.Id)
        };

        var totalItems = await usersQuery.CountAsync();

        var users = dto.PageSize <= 0
            ? await usersQuery.ToListAsync()
            : await usersQuery
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

        return Ok(new
        {
            page = dto.Page,
            pageSize = dto.PageSize,
            totalItems,
            totalPages = dto.PageSize <= 0
                ? 1
                : (int)Math.Ceiling(totalItems / (double)dto.PageSize),
            data = users
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userExist = await _service.GetById(id);
        if (userExist == null)
            return BadRequest(new { message = "User not exists" });
        return Ok(userExist);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var emailExists = await _service.GetByEmail(dto.Email);

        if (emailExists != null)
            return BadRequest(new { message = "Email already exists" });

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = PasswordHelper.Hash(dto.Password),
            Role = dto.Role
        };

        _service.Create(user);
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _service.GetById(id);

        if (user == null)
            return NotFound(new { message = "User not found" });

        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.Role = dto.Role;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = PasswordHelper.Hash(dto.Password);
        }

        _service.Update(user);

        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _service.GetById(id);

        if (user == null)
            return NotFound(new { message = "User not found" });
        await _service.Delete(id);
        return Ok("Deleted");
    }
}