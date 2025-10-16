using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }
        public string? Role { get; set; }
        public string? Module { get; set; }
        public string? PermissionName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
