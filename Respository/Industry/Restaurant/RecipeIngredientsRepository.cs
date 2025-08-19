using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class RecipeIngredientsRepository(ApiDbContext dbContext): IRecipeIngredients
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task<RecipeIngredients?> CreateRecipeIngredientsAsync(RecipeIngredients recipeIngredients)
        {
            await _dbContext.RecipeIngredientss.AddAsync(recipeIngredients);
            await _dbContext.SaveChangesAsync();
            return recipeIngredients;
        }

        public async Task<RecipeIngredients?> DeleteRecipeIngredientsAsync(int id)
        {
            var recipe = await _dbContext.RecipeIngredientss.FirstOrDefaultAsync(r => r.RecipeIngredientsId == id);

            if (recipe == null) return null;

            _dbContext.RecipeIngredientss.Remove(recipe);
            await _dbContext.SaveChangesAsync();
            return recipe;
        }

        public async Task<List<RecipeIngredients>> GetAllRecipeIngredientsAsync()
        {
            return await _dbContext.RecipeIngredientss.ToListAsync();
        }

        public async Task<RecipeIngredients?> GetRecipeIngredientsByIdAsync(int id)
        {
            var recipe = await _dbContext.RecipeIngredientss.FirstOrDefaultAsync(r => r.RecipeIngredientsId == id);

            if (recipe == null) return null;
            return recipe;
        }
    }
}
