using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.DTOs
{
    public class CategoryCreateDTO
    {
        [Required]
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public int? StoreId { get; set; } // Nullable for general categories
    }
    
    public class CategoryUpdateDTO
    {
        [Required]
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public int? StoreId { get; set; } // Nullable for general categories
    }
    
    public class CategoryResponseDTO
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}