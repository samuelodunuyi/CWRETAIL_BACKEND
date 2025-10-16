namespace CWSERVER.Models.Industries.Restaurant.DTOs.Permissions
{
    public class GetPermissionsDTO
    {
        public int PermissionId { get; set; }
        public string? Role { get; set; }
        public string? Module { get; set; }
        public string? PermissionName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
