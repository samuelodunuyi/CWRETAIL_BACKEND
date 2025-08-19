namespace CWSERVER.Models.Core.DTOs
{
    public class ProductCreateDTO
    {
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }  // required
        public int StoreId { get; set; }     // required

        public string? ProductLabel { get; set; }
        public int ProductAmountInStock { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductSKU { get; set; }
        public int LowStockWarningCount { get; set; } = 0;
        public bool Status { get; set; } = true;

        public List<int>? DeleteImageIds { get; set; }
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