using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CW_RETAIL.Models.Core;

namespace CW_RETAIL.Models.Industries
{
    public class Recipe
    {
        [Key]
        public int RecipeId { get; set; }
        
        [ForeignKey("RestaurantProduct")]
        public int ProductId { get; set; }
        
        public virtual RestaurantProduct? Product { get; set; }
        
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        
        public virtual Store? Store { get; set; }
        
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
        public int? CreatedBy { get; set; }
        
        public virtual User? User { get; set; }
        
        public int Status { get; set; }
        
        public int Version { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }
}