using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.ProductVariants
{
    public class UpdateProductVariantsDTO
    {
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public string? VariantName { get; set; }
        public string? VariantType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceModifier { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsAvailable { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
