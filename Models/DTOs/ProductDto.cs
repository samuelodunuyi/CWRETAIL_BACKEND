namespace CWSERVER.Models.DTOs
{
    public class ProductCreateDTO
    {
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public int StoreId { get; set; }
        public string? ProductLabel { get; set; }
        public int ProductAmountInStock { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductSKU { get; set; }
       
    }

    public class ProductResponseDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? MainImageUrl { get; set; }
        public List<string>? AdditionalImageUrls { get; set; }
        public string? ProductLabel { get; set; }
        public int ProductAmountInStock { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductSKU { get; set; }
    }
}