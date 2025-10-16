using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class Ingredients
    {
        [Key]
        public int IngredientId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        [ForeignKey("Stores")]
        public int StoreId { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal MinimumStockLevel { get; set; }
        public string? UnitOfMeasure { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal CostPerUnit { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal CaloriesPerUnit { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal ? ProteinPerUnit { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal ? CarbohydratesPerUnit { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal ? FatsPerUnit { get; set; }
        public string? Allergens { get; set; }
        public bool IsVegetarian { get; set; } = false;
        public bool IsVegan { get; set; } = false;
        public bool IsGlutenFree { get; set; } = false;
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
