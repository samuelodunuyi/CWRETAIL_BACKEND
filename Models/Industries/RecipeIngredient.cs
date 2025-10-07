using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CW_RETAIL.Models.Industries
{
    public class RecipeIngredient
    {
        [Key]
        public int RecipeIngredientId { get; set; }
        
        [ForeignKey("Recipe")]
        public int RecipeId { get; set; }
        
        [JsonIgnore]
        public virtual Recipe? Recipe { get; set; }
        
        [ForeignKey("RestaurantProduct")]
        public int IngredientId { get; set; }
        
        public virtual RestaurantProduct? Ingredient { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }
        
        public string? UnitOfMeasure { get; set; }
        
        public bool IsOptional { get; set; }
        
        public string? PreparationNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}