using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/order-details")]
public class OrderDetailsController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderDetailsController(AppDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách product của user hiện tại
    //[HttpGet]
    //public async Task<IActionResult> GetMyProducts([FromQuery] OrderQueryDto dto)
    //{
    //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //    if (userId == null)
    //        return Unauthorized();

    //    var products = await _context.OrderDetails
    //        .Where(od => od.UserId == int.Parse(userId))
    //        .Include(od => od.Product)
    //        .OrderByDescending(od => od.CreatedAt)
    //        .Select(od => new
    //        {
    //            od.Product.Id,
    //            od.Product.Name,
    //            od.Product.Price,
    //            ProductQuantity = od.Product.Quantity,
    //            OrderQuantity = od.Quantity,
    //            OrderTotalPrice = od.TotalPrice,
    //            od.CreatedAt
    //        })
    //        .ToListAsync();

    //    return Ok(products);
    //}

    [HttpGet]
    public async Task<IActionResult> GetMyProducts([FromQuery] OrderQueryDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var userIdInt = int.Parse(userId);

        var ordersQuery = _context.OrderDetails
            .Include(od => od.Product)
            .Include(od => od.User)
            .Where(od => od.UserId == userIdInt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Keyword))
        {
            ordersQuery = ordersQuery.Where(od =>
                od.Product.Name.Contains(dto.Keyword)||
                od.User.FullName.Contains(dto.Keyword) ||
                od.User.Email.Contains(dto.Keyword));
        }

        ordersQuery = ordersQuery
            .OrderByDescending(od => od.CreatedAt);

        var totalItems = await ordersQuery.CountAsync();

        var orders = dto.PageSize <= 0
            ? await ordersQuery
                .Select(od => new
                {
                    id = od.Id,

                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    ProductPrice = od.Product.Price,
                    ProductQuantity = od.Product.Quantity,

                    OrderQuantity = od.Quantity,
                    OrderTotalPrice = od.TotalPrice,
                    od.CreatedAt
                })
                .ToListAsync()
            : await ordersQuery
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(od => new
                {
                    id = od.Id,

                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    ProductPrice = od.Product.Price,
                    ProductQuantity = od.Product.Quantity,

                    OrderQuantity = od.Quantity,
                    OrderTotalPrice = od.TotalPrice,
                    od.CreatedAt
                })
                .ToListAsync();

        return Ok(new
        {
            page = dto.Page,
            pageSize = dto.PageSize,
            totalItems,
            totalPages = dto.PageSize <= 0
                ? 1
                : (int)Math.Ceiling(totalItems / (double)dto.PageSize),
            data = orders
        });
    }

    // User mua product
    [HttpPost]
    public async Task<IActionResult> BuyProduct([FromBody] BuyProductDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var userIdInt = int.Parse(userId);

        var product = await _context.Products.FindAsync(dto.ProductId);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found"
            });
        }

        if (dto.Quantity <= 0)
        {
            return BadRequest(new
            {
                message = "Quantity must be at least 1"
            });
        }

        if (product.Quantity < dto.Quantity)
        {
            return BadRequest(new
            {
                message = "Not enough stock"
            });
        }

        var order = new OrderDetails
        {
            UserId = userIdInt,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            TotalPrice = dto.Quantity * product.Price
        };

        _context.OrderDetails.Add(order);

        product.Quantity -= dto.Quantity;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Buy product successfully"
        });
    }

    // Xóa product khỏi user
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveProduct(int productId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var order = await _context.OrderDetails
            .FirstOrDefaultAsync(od =>
                od.UserId == int.Parse(userId) &&
                od.ProductId == productId);

        if (order == null)
            return NotFound(new { message = "Order not found in your profile" });

        _context.OrderDetails.Remove(order);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Remove product successfully"
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllOrder([FromQuery] OrderQueryDto dto)
    {
        var ordersQuery = _context.OrderDetails
            .Include(od => od.User)
            .Include(od => od.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Keyword))
        {
            ordersQuery = ordersQuery.Where(od =>
                od.User.FullName.Contains(dto.Keyword) ||
                od.User.Email.Contains(dto.Keyword) ||
                od.Product.Name.Contains(dto.Keyword));
        }

        ordersQuery = ordersQuery
            .OrderByDescending(od => od.CreatedAt);

        var totalItems = await ordersQuery.CountAsync();

        var orders = dto.PageSize <= 0
            ? await ordersQuery
                .Select(od => new
                {
                    id = od.Id,
                    UserId = od.UserId,
                    FullName = od.User.FullName,
                    Email = od.User.Email,

                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    ProductPrice = od.Product.Price,
                    ProductQuantity = od.Product.Quantity,

                    OrderQuantity = od.Quantity,
                    OrderTotalPrice = od.TotalPrice,
                    od.CreatedAt
                })
                .ToListAsync()
            : await ordersQuery
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(od => new
                {
                    id = od.Id,
                    UserId = od.UserId,
                    FullName = od.User.FullName,
                    Email = od.User.Email,

                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    ProductPrice = od.Product.Price,
                    ProductQuantity = od.Product.Quantity,

                    OrderQuantity = od.Quantity,
                    OrderTotalPrice = od.TotalPrice,
                    od.CreatedAt
                })
                .ToListAsync();

        return Ok(new
        {
            page = dto.Page,
            pageSize = dto.PageSize,
            totalItems,
            totalPages = dto.PageSize <= 0
                ? 1
                : (int)Math.Ceiling(totalItems / (double)dto.PageSize),
            data = orders
        });
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("admin")]
    public async Task<IActionResult> AddOrder([FromBody] AddOrderDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
            return NotFound(new
            {
                message = "User not found"
            });

        var product = await _context.Products.FindAsync(dto.ProductId);

        if (product == null)
            return NotFound(new
            {
                message = "Product not found"
            });

        if (dto.Quantity <= 0)
        {
            return BadRequest(new { message = "Quantity must be at least 1" });
        }

        if (product.Quantity < dto.Quantity)
        {
            return BadRequest(new { message = "Not enough stock" });
        }

        var order = new OrderDetails
        {
            UserId = dto.UserId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            TotalPrice = dto.Quantity * product.Price
        };

        _context.OrderDetails.Add(order);

        product.Quantity -= dto.Quantity;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Add product to user successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("admin")]
    public async Task<IActionResult> RemoveOrder(
    [FromQuery] int userId,
    [FromQuery] int productId)
    {
        var order = await _context.OrderDetails
            .FirstOrDefaultAsync(od =>
                od.UserId == userId &&
                od.ProductId == productId);

        if (order == null)
        {
            return NotFound(new { message = "OrderDetails not found" });
        }

        _context.OrderDetails.Remove(order);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Remove product from user successfully"
        });
    }
}