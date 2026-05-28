using System.ComponentModel.DataAnnotations;

public class UpdateProductDto
{
    [Required]
    public string Name { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}