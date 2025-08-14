using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.ProductVariants
{
    public class GetProductVariantsDTO
    {
        public int ProductVariantId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public string? VariantName { get; set; }
        public string? VariantType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceModifier { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsAvailable { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
