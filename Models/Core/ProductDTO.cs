using System.Collections.Generic;

namespace CW_RETAIL.Models.Core
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int StoreId { get; set; }
        public decimal BasePrice { get; set; }
        public int CurrentStock { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<ProductImage>? AdditionalImages { get; set; }
        public bool ShowInWeb { get; set; }
        public bool ShowInPOS { get; set; }
        public bool IsActive { get; set; }
        
        // Store information - will be populated based on user role
        public object? Store { get; set; }
        
        // User information - will be populated based on user role
        public UserBasicInfoDTO? CreatedByUser { get; set; }
    }
}