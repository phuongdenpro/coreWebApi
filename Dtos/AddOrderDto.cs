using System.ComponentModel.DataAnnotations;

public class AddOrderDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}