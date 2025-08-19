using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class PermissionsRepository(ApiDbContext dbContext) : IPermissions
    {
        private readonly ApiDbContext _dbContext = dbContext;
        public async Task<Permissions?> CreatePermissionsAsync(Permissions permissions)
        {
            await _dbContext.Permissionss.AddAsync(permissions);
            await _dbContext.SaveChangesAsync();
            return permissions;
        }

        public async Task<Permissions?> DeletePermissionAsync(int id)
        {
            var permission = await _dbContext.Permissionss.FirstOrDefaultAsync(per => per.PermissionId == id);

            if (permission == null)
                return null;

            _dbContext.Permissionss.Remove(permission);
            await _dbContext.SaveChangesAsync();
            return permission;
        }

        public async Task<List<Permissions>> GetAllPermissionsAsync()
        {
            return await _dbContext.Permissionss.ToListAsync();
        }

        public async Task<Permissions?> GetPermissionsByIdAsync(int id)
        {
            var permission = await _dbContext.Permissionss.FirstOrDefaultAsync(per => per.PermissionId == id);

            if (permission == null)
                return null;

            return permission;
        }

        public Task<Permissions?> GetPermissionsByUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
