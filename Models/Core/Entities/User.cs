using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CWSERVER.Models.Core.Entities
{
    public class User : IdentityUser
    {
        [Required]
        public string Role { get; set; } = "Customer"; 

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedBy { get; set; }
    }
}