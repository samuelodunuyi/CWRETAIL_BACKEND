using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CW_RETAIL.Models.Core;

namespace CW_RETAIL.Models.Industries
{
    public class RestaurantProduct : Product
    {
        public bool IsRecipe { get; set; }
        
        public int PrepTimeMinutes { get; set; }
        
        public int CookingTimeMinutes { get; set; }
        
        public string? Allergens { get; set; }
        
        public int SpiceLevel { get; set; }
        
        public List<string> DietTypes { get; set; } = new List<string>(); // paleo, whole30, vegan, gluten-free
        
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}