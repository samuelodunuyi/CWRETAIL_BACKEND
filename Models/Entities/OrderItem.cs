using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductCategory { get; set; }
        public decimal PriceAtOrder { get; set; }
        public decimal? OriginalPriceAtOrder { get; set; }
        public int Quantity { get; set; }
        public string? ProductImageUrl { get; set; } 
    }
}