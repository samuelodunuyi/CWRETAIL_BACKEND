using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.DTOs
{
    public class ProductCreateDTO
    {
        [Required]
        public string? ProductName { get; set; }
        
        [Required]
        public int CategoryId { get; set; }  // required
        
        [Required]
        public int StoreId { get; set; }     // required

        public string? ProductLabel { get; set; }
        // ProductAmountInStock removed - will default to 0
        
        [Required]
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        
        [Required]
        public string? ProductSKU { get; set; }
        public int LowStockWarningCount { get; set; } = 0;
        public bool Status { get; set; } = true;

        public List<int>? DeleteImageIds { get; set; }
    }
    
    public class ProductUpdateDTO
    {
        [Required]
        public string? ProductName { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public int StoreId { get; set; }

        public string? ProductLabel { get; set; }
        // ProductAmountInStock removed - use restock endpoint
        
        [Required]
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        
        [Required]
        public string? ProductSKU { get; set; }
        public int LowStockWarningCount { get; set; } = 0;
        public bool Status { get; set; } = true;

        public List<int>? DeleteImageIds { get; set; }
    }
    
    public class ProductRestockDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Stock amount must be greater than 0")]
        public int StockAmount { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount to add must be greater than 0")]
        public int AmountToAdd { get; set; }
    }


    public class ProductResponseDTO
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? MainImageUrl { get; set; }

        public List<ProductImageDTO> AdditionalImages { get; set; } = new();

        public string? ProductLabel { get; set; }
        public int ProductAmountInStock { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductSKU { get; set; }
        public int LowStockWarningCount { get; set; } = 0;
        public bool Status { get; set; } = true;
    }

    public class ProductImageDTO
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

}