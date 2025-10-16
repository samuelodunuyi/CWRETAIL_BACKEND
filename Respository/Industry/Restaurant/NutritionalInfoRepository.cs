using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.NutriionalInfo;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class NutritionalInfoRepository(ApiDbContext dbContext) : INutritionalInfo
    {
        private readonly ApiDbContext _dbContext = dbContext;
        public async Task<NutritionalInfo?> CreateNutritionalInfoAsync(NutritionalInfo nutritionalInfo)
        {
            await _dbContext.NutritionalInfos.AddAsync(nutritionalInfo);
            await _dbContext.SaveChangesAsync();
            return nutritionalInfo;
        }

        public async Task<NutritionalInfo?> DeleteNutritionalInfoAsync(int id)
        {
            var info = await _dbContext.NutritionalInfos.FirstOrDefaultAsync(inf => inf.NutritionalInfoId == id);

            if (info == null)
                return null;

            _dbContext.NutritionalInfos.Remove(info);
            await _dbContext.SaveChangesAsync();
            return info;
        }

        public async Task<List<NutritionalInfo>> GetAllNutritionalInfoAsync()
        {
            return await _dbContext.NutritionalInfos.ToListAsync();
        }

        public async Task<NutritionalInfo?> GetNutritionalInfoByIdAsync(int id)
        {
            var info = await _dbContext.NutritionalInfos.FirstOrDefaultAsync(inf => inf.NutritionalInfoId == id);

            if (info == null)
                return null;

            return info;
        }

        public async Task<NutritionalInfo?> UpdateNutritionalInfoAsync(int id, UpdateNutritionalInfo updateNutritionalInfo)
        {
            var info = await _dbContext.NutritionalInfos.FirstOrDefaultAsync(inf => inf.NutritionalInfoId == id);

            if (info == null)
                return null;

            info.ProductId = updateNutritionalInfo.ProductId;
            info.ServingSize = updateNutritionalInfo.ServingSize;
            info.Calories = updateNutritionalInfo.Calories;
            info.Protein = updateNutritionalInfo.Protein;
            info.Carbohydrates = updateNutritionalInfo.Carbohydrates;
            info.TotalFat = updateNutritionalInfo.TotalFat;
            info.SaturatedFat = updateNutritionalInfo.SaturatedFat;
            info.TransFat = updateNutritionalInfo.TransFat;
            info.Cholesterol = updateNutritionalInfo.Cholesterol;
            info.Sodium = updateNutritionalInfo.Sodium;
            info.DietaryFiber = updateNutritionalInfo.DietaryFiber;
            info.Sugars = updateNutritionalInfo.Sugars;
            info.UpdatedAt = updateNutritionalInfo.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return info;
        }
    }
}
