using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CWSERVER.Models.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        
        
        [JsonIgnore]
        public virtual Order? Order { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductCategory { get; set; }
        [Column(TypeName="decimal(18, 2)")]
        public decimal PriceAtOrder { get; set; }
        [Column(TypeName="decimal(18, 2)")]
        public decimal? OriginalPriceAtOrder { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public int Quantity { get; set; }
        public string? ProductImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}