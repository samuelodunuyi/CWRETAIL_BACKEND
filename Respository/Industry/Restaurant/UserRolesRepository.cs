using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class UserRolesRepository(ApiDbContext dbContext): IUserRoles
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task<UserRoles?> CreateUserRolesAsync(UserRoles userRoles)
        {
            await _dbContext.UserRoless.AddAsync(userRoles);
            await _dbContext.SaveChangesAsync();
            return userRoles;
        }

        public async Task<UserRoles?> DeleteUserRoleAsync(int id)
        {
            var roles = await _dbContext.UserRoless.FirstOrDefaultAsync(r => r.UserRolesId == id);

            if (roles == null) return null;

            _dbContext.UserRoless.Remove(roles);
            await _dbContext.SaveChangesAsync();
            return roles;
        }

        public async Task<List<UserRoles>> GetAllUserRolesAsync()
        {
            return await _dbContext.UserRoless.ToListAsync();
        }

        public async Task<UserRoles?> GetUserRolesById(int id)
        {
            var roles = await _dbContext.UserRoless.FirstOrDefaultAsync(r => r.UserRolesId == id);

            if (roles == null) return null;

            return roles;
        }
    }
}
