using CWSERVER.Models.Industries.Restaurant.DTOs.RecipeIngredients;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IRecipeIngredients
    {
        Task<List<RecipeIngredients>> GetAllRecipeIngredientsAsync();
        Task<RecipeIngredients?> GetRecipeIngredientsByIdAsync(int id);
        Task<RecipeIngredients?> CreateRecipeIngredientsAsync(RecipeIngredients recipeIngredients);
        Task<RecipeIngredients?> DeleteRecipeIngredientsAsync(int id);
    }
}
