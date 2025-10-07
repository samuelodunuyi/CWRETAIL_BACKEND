using System.Collections.Generic;
using CW_RETAIL.Models.Core;

namespace CW_RETAIL.Models.Industries
{
    public class RestaurantProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public int CategoryId { get; set; }
        public int StoreId { get; set; }
        public decimal BasePrice { get; set; }
        public int CurrentStock { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? ImageUrl { get; set; }
        public bool ShowInWeb { get; set; }
        public bool ShowInPOS { get; set; }
        public bool IsActive { get; set; }
        
        // Restaurant specific properties
        public bool IsRecipe { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public string? Allergens { get; set; }
        public int SpiceLevel { get; set; }
        public List<string> DietTypes { get; set; } = new List<string>();
        
        // Limited store information for non-admin users
        public StoreBasicInfoDTO Store { get; set; } = new StoreBasicInfoDTO();
    }
    
    public class StoreBasicInfoDTO
    {
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? StoreAddress { get; set; }
    }
    
    public class StoreFullInfoDTO : StoreBasicInfoDTO
    {
        public string? StorePhoneNumber { get; set; }
        public string? StoreEmailAddress { get; set; }
        public string? StoreAdmin { get; set; }
        public string StoreType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    
    public class UserBasicInfoDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}