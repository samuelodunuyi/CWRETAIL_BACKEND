using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.Recipes
{
    public class CreateRecipesDTO
    {
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [ForeignKey("Stores")]
        public int StoreId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Instructions { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public int Servings { get; set; }
        public int Difficulty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EstimatedCost { get; set; }
        [ForeignKey("User")]
        public string? CreatedBy { get; set; }
        public string? Status { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
