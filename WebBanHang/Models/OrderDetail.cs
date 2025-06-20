using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebBanHang.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        // Navigation properties
        [ForeignKey("OrderId")]
        [ValidateNever]
        public Order Order { get; set; }
        
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
    }
}
