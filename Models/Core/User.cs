using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CW_RETAIL.Models.Core
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        public string PasswordSalt { get; set; }
        
        [Required]
        public int RoleId { get; set; }
        
        public virtual UserRole Role { get; set; }
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}