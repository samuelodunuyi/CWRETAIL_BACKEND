using CWSERVER.Models.Industries.Restaurant.DTOs.Recipes;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IRecipes
    {
        Task<List<Recipes>> GetAllRecipesAsync();
        Task<Recipes?> GetRecipesByIdAsync(int id);
        Task<Recipes?> CreateRecipesAsync(Recipes recipes);
        Task<Recipes?> UpdateRecipesByIdAsync(int id, UpdateRecipesDTO updateRecipesDTO);
        Task<Recipes?> DeleteRecipesByIdAsync(int id);
    }
}
