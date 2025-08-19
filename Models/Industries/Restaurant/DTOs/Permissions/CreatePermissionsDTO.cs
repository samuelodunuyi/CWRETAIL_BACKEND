namespace CWSERVER.Models.Industries.Restaurant.DTOs.Permissions
{
    public class CreatePermissionsDTO
    {
        public string? Role { get; set; }
        public string? Module { get; set; }
        public string? PermissionName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
