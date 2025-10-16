using CWSERVER.Models.Industries.Restaurant.DTOs.Permissions;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IPermissions
    {
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<Permission?> GetPermissionsByIdAsync(int id);
        Task<Permission?> GetPermissionsByUserIdAsync(int userId);
        Task<Permission?> CreatePermissionsAsync(Permission permissions);
        Task<Permission?> DeletePermissionAsync(int id);
    }
}
