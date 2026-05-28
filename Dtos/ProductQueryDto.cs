public class ProductQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Keyword { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortByPrice { get; set; }
}