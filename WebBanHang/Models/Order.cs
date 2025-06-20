using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebBanHang.Models
{    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}