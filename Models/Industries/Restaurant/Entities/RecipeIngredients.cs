using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class RecipeIngredients
    {
        [Key]
        public int RecipeIngredientsId { get; set; }
        [ForeignKey("Recipes")]
        public int RecipeId { get; set; }
        [ForeignKey("Ingredients")]
        public int IngredientsId { get; set; }
        [Column(TypeName ="decimal(18, 2)")]
        public decimal Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsOptional { get; set; }
        public string? PreparationNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
