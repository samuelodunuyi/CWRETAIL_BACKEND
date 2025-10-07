using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CW_RETAIL.Models.Core
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        
        [JsonIgnore]
        public virtual Order? Order { get; set; }
        
        public int ProductId { get; set; }
        
        public string? ProductName { get; set; }
        
        public string? ProductDescription { get; set; }
        
        public string? ProductCategory { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceAtOrder { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? OriginalPriceAtOrder { get; set; }
        
        public int Quantity { get; set; }
        
        public string? ProductImageUrl { get; set; }
    }
}