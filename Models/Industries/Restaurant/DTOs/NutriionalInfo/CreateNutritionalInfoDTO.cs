using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.NutriionalInfo
{
    public class CreateNutritionalInfoDTO
    {
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public string? ServingSize { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Calories { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Protein { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Carbohydrates { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalFat { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaturatedFat { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TransFat { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cholesterol { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Sodium { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DietaryFiber { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Sugars { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
