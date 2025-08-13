using CWSERVER.Models.Industries.Restaurant.DTOs.Ingredients;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IIngredients
    {
        Task<List<Ingredients>> GetAllIngredientsAsync();
        Task<Ingredients?> GetIngredientsByIdAsync(int id);
        Task<Ingredients?> CreateIngredientsAsync(Ingredients ingredients);
        Task<Ingredients?> UpdateIngredientsAsync(int id, UpdateIngredientsDTO updateIngredientsDTO);
        Task<Ingredients?> DeleteIngredientsAsync(int id);
    }
}
