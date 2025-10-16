using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.DTOs
{
    public class EmployeeCreateDTO
    {
        [Required]
        public string? UserId { get; set; }
        
        public int? StoreId { get; set; }
        
        [Required]
        public string? FirstName { get; set; }
        
        [Required]
        public string? LastName { get; set; }
        
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }
    }
    
    public class EmployeeUpdateDTO
    {
        [Required]
        public string? UserId { get; set; }
        
        public int? StoreId { get; set; }
        
        [Required]
        public string? FirstName { get; set; }
        
        [Required]
        public string? LastName { get; set; }
        
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }
    }
    
    public class EmployeeResponseDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
