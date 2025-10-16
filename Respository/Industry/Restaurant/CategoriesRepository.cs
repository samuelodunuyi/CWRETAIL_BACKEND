using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Categories;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class CategoriesRepository(ApiDbContext dbContext) : ICategories
    {
        private readonly ApiDbContext _dbContext = dbContext;
        public async Task<Categories?> CreateCategoriesAsync(Categories categories)
        {
            await _dbContext.Categoriess.AddAsync(categories);
            await _dbContext.SaveChangesAsync();
            return categories;
        }

        public async Task<Categories?> DeleteCategoriesAsync(int id)
        {
            var category = await _dbContext.Categoriess.FirstOrDefaultAsync(cat => cat.CategoryId == id);

            if (category == null)
            {
                return null;
            }

            _dbContext.Categoriess.Remove(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categoriess.ToListAsync();
        }

        public async Task<Categories?> GetCategoriesByIdAsync(int id)
        {
            var category = await _dbContext.Categoriess.FirstOrDefaultAsync(cat => cat.CategoryId == id);

            if (category == null)
            {
                return null;
            }

            return category;
        }

        public async Task<Categories?> UpdateCategoriesAsync(int id, UpdateCategoriesDTO updateCategoriesDTO)
        {
            var category = await _dbContext.Categoriess.FirstOrDefaultAsync(cat => cat.CategoryId == id);

            if (category == null)
                return null;

            category.Name = updateCategoriesDTO.Name;
            category.Description = updateCategoriesDTO.Description;
            category.StoreId = updateCategoriesDTO.StoreId;
            category.DisplayOrder = updateCategoriesDTO.DisplayOrder;
            category.IsActive = updateCategoriesDTO.IsActive;
            category.UpdatedAt = updateCategoriesDTO.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return category;
        }
    }
}
