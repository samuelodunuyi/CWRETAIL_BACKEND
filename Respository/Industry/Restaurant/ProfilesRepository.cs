using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Profiles;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class ProfilesRepository(ApiDbContext dbContext): IProfiles
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task<Profiles?> CreateProfileAsync(Profiles profiles)
        {
            await _dbContext.Profiless.AddAsync(profiles);
            await _dbContext.SaveChangesAsync();  
            return profiles;
        }

        public async Task<Profiles?> DeleteProfileAsync(int id)
        {
            var profile = await _dbContext.Profiless.FirstOrDefaultAsync(p => p.ProfileId == id);

            if (profile == null) return null;

            _dbContext.Profiless.Remove(profile);
            await _dbContext.SaveChangesAsync();
            return profile;
        }

        public async Task<List<Profiles>> GetAllProfileAsync()
        {
            return await _dbContext.Profiless.ToListAsync();
        }

        public async Task<Profiles?> GetProfilesByIdAsync(int id)
        {
            var profile = await _dbContext.Profiless.FirstOrDefaultAsync(p => p.ProfileId == id);

            if (profile == null) return null;
            return profile;
        }

        public async Task<Profiles?> UpdateProfileAsync(int id, UpdateProfilesDTO updateProfilesDTO)
        {
            var profile = await _dbContext.Profiless.FirstOrDefaultAsync(p => p.ProfileId == id);

            if (profile == null) return null;

            profile.FirstName = updateProfilesDTO.FirstName;
            profile.LastName = updateProfilesDTO.LastName;
            profile.Phone = updateProfilesDTO.Phone;
            profile.Email = updateProfilesDTO.Email;
            profile.AvatarURL = updateProfilesDTO.AvatarURL;
            profile.UpdatedAt = updateProfilesDTO.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return profile;
        }
    }
}
