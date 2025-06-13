using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string ImageUrl { get; set; } = string.Empty;
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
        
        // Foreign key to User
        public string? UserId { get; set; }
        
        // Session ID for guest users
        public string? SessionId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [NotMapped]
        public decimal Total => Price * Quantity;
        
        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
