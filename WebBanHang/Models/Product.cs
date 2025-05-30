using System.ComponentModel.DataAnnotations;

public class Product
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Range(1.00, 500000000.00)]
    public decimal Price { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    
    public List<ProductImage>? Images { get; set; }
    
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }
}