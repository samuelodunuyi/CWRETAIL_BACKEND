using CWSERVER.Models.Industries.Restaurant.DTOs.Profiles;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IProfiles
    {
        Task<List<Profiles>> GetAllProfileAsync();
        Task<Profiles?> GetProfilesByIdAsync(int id);
        Task<Profiles?> CreateProfileAsync(Profiles profiles);
        Task<Profiles?> UpdateProfileAsync(int id, UpdateProfilesDTO updateProfilesDTO);
        Task<Profiles?> DeleteProfileAsync(int id);
    }
}
