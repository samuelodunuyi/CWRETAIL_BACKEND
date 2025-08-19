using CWSERVER.Models.Industries.Restaurant.DTOs.Permissions;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IPermissions
    {
        Task<List<Permissions>> GetAllPermissionsAsync();
        Task<Permissions?> GetPermissionsByIdAsync(int id);
        Task<Permissions?> GetPermissionsByUserIdAsync(int userId);
        Task<Permissions?> CreatePermissionsAsync(Permissions permissions);
        Task<Permissions?> DeletePermissionAsync(int id);
    }
}
