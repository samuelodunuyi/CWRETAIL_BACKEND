using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.Products
{
    public class CreateProductsDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [ForeignKey("Stores")]
        public int StoreId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasePrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsRecipe { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public string? Allergens { get; set; }
        public int SpiceLevel { get; set; }
        public bool IsVegetarian { get; set; }
        public bool IsVegan { get; set; }
        public bool IsGlutenFree { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
