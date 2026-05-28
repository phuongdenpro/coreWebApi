using coreWebApi.Models;

namespace DemoApi.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }

    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
}