using CWSERVER.Models.Industries.Restaurant.DTOs.Categories;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface ICategories
    {
        Task<List<Categories>> GetAllCategoriesAsync();
        Task<Categories?> GetCategoriesByIdAsync(int id);
        Task<Categories?> CreateCategoriesAsync(Categories categories);
        Task<Categories?> UpdateCategoriesAsync(int id, UpdateCategoriesDTO updateCategoriesDTO);
        Task<Categories?> DeleteCategoriesAsync(int id);
    }
}
