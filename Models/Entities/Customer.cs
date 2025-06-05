using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public virtual User? User { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        public string? CreatedBy { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}