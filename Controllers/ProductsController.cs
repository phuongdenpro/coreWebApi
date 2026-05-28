using coreWebApi.Models;
using coreWebApi.Services.Interfaces;
using DemoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace coreWebApi.Controllers;

[ApiController]
[Route("api/admin/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly S3Service _s3Service;
    private const string DefaultImageUrl = "https://cdn.example.com/default-product.png";


    public ProductsController(AppDbContext context, S3Service s3Service)
    {
        _context = context;
        _s3Service = s3Service;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryDto dto)
    {
        var productsQuery = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Keyword))
        {
            productsQuery = productsQuery
                .Where(p => p.Name.Contains(dto.Keyword));
        }

        if (dto.MinPrice.HasValue)
        {
            productsQuery = productsQuery
                .Where(p => p.Price >= dto.MinPrice.Value);
        }

        if (dto.MaxPrice.HasValue)
        {
            productsQuery = productsQuery
                .Where(p => p.Price <= dto.MaxPrice.Value);
        }

        if (dto.SortByPrice == "asc")
        {
            productsQuery = productsQuery.OrderBy(p => p.Price);
        }
        else if (dto.SortByPrice == "desc")
        {
            productsQuery = productsQuery.OrderByDescending(p => p.Price);
        }
        else
        {
            productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
        }

        var totalItems = await productsQuery.CountAsync();

        var queryResult = productsQuery.Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.Quantity,
            p.Description,
            ImageUrl = p.ImageUrl ?? DefaultImageUrl,
            p.CreatedAt
        });

        var products = dto.PageSize <= 0
            ? await queryResult.ToListAsync()
            : await queryResult
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
            data = products
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var productExists = await _context.Products
         .AnyAsync(p => p.Name == dto.Name);

        if (productExists)
        {
            return BadRequest(new
            {
                message = "Product already exists"
            });
        }

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Description = dto?.Description,
            ImageUrl = dto?.ImageUrl ?? DefaultImageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();


        return Ok(new
        {
            message = "Product added successfully",
            product
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        product.Description = dto?.Description;
        product.ImageUrl = dto?.ImageUrl ?? DefaultImageUrl;

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _context.Products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Quantity,
                p.Description,
                ImageUrl = p.ImageUrl ?? DefaultImageUrl,
                p.CreatedAt
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Delete product successfully" });
    }

    [HttpPost("{id}/upload-image")]
    public async Task<IActionResult> UploadProductImage(int id, IFormFile image)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound(new { message = "Không tìm thấy sản phẩm" });

        if (image == null || image.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn ảnh" });

        var imageUrl = await _s3Service.UploadFileAsync(image, "products");

        product.ImageUrl = imageUrl;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Upload ảnh thành công",
            imageUrl = imageUrl,
            product = product
        });
    }
}