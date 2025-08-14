using CWSERVER.Models.Industries.Restaurant.DTOs.UserRoles;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IUserRoles
    {
        Task<List<UserRoles>> GetAllUserRolesAsync();
        Task<UserRoles?> GetUserRolesByIdAsync(int id);
        Task<UserRoles?> CreateUserRolesAsync(UserRoles userRoles);
        Task<UserRoles?> DeleteUserRoleAsync(int id);
    }
}
