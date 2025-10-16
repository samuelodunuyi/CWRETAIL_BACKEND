using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey("Store")]
        public int? StoreId { get; set; }
        public virtual Store? Store { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}