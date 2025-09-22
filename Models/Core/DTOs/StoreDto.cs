using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.DTOs
{
    public class StoreCreateDTO
    {
        [Required]
        public string? StoreName { get; set; }
        public string? StoreRep { get; set; }
    }
    
    public class StoreUpdateDTO
    {
        [Required]
        public string? StoreName { get; set; }
        public string? StoreRep { get; set; }
    }
    
    public class StoreResponseDTO
    {
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? StoreRep { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}