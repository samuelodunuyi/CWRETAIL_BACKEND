using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Recipes;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class RecipesRepository(ApiDbContext dbContext): IRecipes
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task<Recipes?> CreateRecipesAsync(Recipes recipes)
        {
            await _dbContext.Recipess.AddAsync(recipes);
            await _dbContext.SaveChangesAsync();
            return recipes;
        }

        public async Task<Recipes?> DeleteRecipesByIdAsync(int id)
        {
            var recipe = await _dbContext.Recipess.FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null) return null;

            _dbContext.Recipess.Remove(recipe);
            await _dbContext.SaveChangesAsync();
            return recipe;
        }

        public async Task<List<Recipes>> GetAllRecipesAsync()
        {
            return await _dbContext.Recipess.ToListAsync();
        }

        public async Task<Recipes?> GetRecipesByIdAsync(int id)
        {
            var recipe = await _dbContext.Recipess.FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null) return null;
            return recipe;
        }

        public async Task<Recipes?> UpdateRecipesByIdAsync(int id, UpdateRecipesDTO updateRecipesDTO)
        {
            var recipe = await _dbContext.Recipess.FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null) return null;

            recipe.ProductId = updateRecipesDTO.ProductId;
            recipe.StoreId = updateRecipesDTO.StoreId;
            recipe.Name = updateRecipesDTO.Name;
            recipe.Description = updateRecipesDTO.Description;
            recipe.Instructions = updateRecipesDTO.Instructions;
            recipe.PrepTimeMinutes = updateRecipesDTO.PrepTimeMinutes;
            recipe.CookingTimeMinutes = updateRecipesDTO.CookingTimeMinutes;
            recipe.Servings = updateRecipesDTO.Servings;
            recipe.Difficulty = updateRecipesDTO.Difficulty;
            recipe.EstimatedCost = updateRecipesDTO.EstimatedCost;
            recipe.CreatedBy = updateRecipesDTO.CreatedBy;
            recipe.Status = updateRecipesDTO.Status;
            recipe.Version = updateRecipesDTO.Version;
            recipe.UpdatedAt = updateRecipesDTO.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return recipe;
        }
    }
}
