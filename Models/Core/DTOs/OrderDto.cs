using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.DTOs
{
    public class OrderCreateDTO
    {
        [Required]
        public int StoreId { get; set; }
        public int? CustomerId { get; set; } // Nullable for walk-in customers
        public int Status { get; set; } = 0;
        
        [Required]
        public List<OrderItemCreateDTO> OrderItems { get; set; } = new();
    }
    
    public class OrderUpdateDTO
    {
        [Required]
        public int StoreId { get; set; }
        public int? CustomerId { get; set; }
        public int Status { get; set; }
    }
    
    public class OrderResponseDTO
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }
        public List<OrderItemResponseDTO> OrderItems { get; set; } = new();
    }
    
    public class OrderItemCreateDTO
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
    
    public class OrderItemResponseDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductCategory { get; set; }
        public decimal PriceAtOrder { get; set; }
        public decimal? OriginalPriceAtOrder { get; set; }
        public int Quantity { get; set; }
        public string? ProductImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}