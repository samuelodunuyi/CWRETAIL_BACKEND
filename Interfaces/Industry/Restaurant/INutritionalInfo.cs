using CWSERVER.Models.Industries.Restaurant.DTOs.NutriionalInfo;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface INutritionalInfo
    {
        Task<List<NutritionalInfo>> GetAllNutritionalInfoAsync();
        Task<NutritionalInfo?> GetNutritionalInfoByIdAsync(int id);
        Task<NutritionalInfo?> CreateNutritionalInfoAsync(NutritionalInfo nutritionalInfo);
        Task<NutritionalInfo?> UpdateNutritionalInfoAsync(int id, UpdateNutritionalInfo updateNutritionalInfo);
        Task<NutritionalInfo?> DeleteNutritionalInfoAsync(int id);
    }
}
