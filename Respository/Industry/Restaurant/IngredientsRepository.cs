using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Ingredients;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class IngredientsRepository(ApiDbContext dbContext) : IIngredients
    {
        private readonly ApiDbContext _dbContext = dbContext;
        public async Task<Ingredients?> CreateIngredientsAsync(Ingredients ingredients)
        {
            await _dbContext.Ingredientss.AddAsync(ingredients);
            await _dbContext.SaveChangesAsync();
            return ingredients;
        }

        public async Task<Ingredients?> DeleteIngredientsAsync(int id)
        {
            var ingredients = await _dbContext.Ingredientss.FirstOrDefaultAsync(ing => ing.IngredientId == id);

            if (ingredients == null) 
                return null;

            _dbContext.Ingredientss.Remove(ingredients);
            await _dbContext.SaveChangesAsync();
            return ingredients;
        }

        public async Task<List<Ingredients>> GetAllIngredientsAsync()
        {
            return await _dbContext.Ingredientss.ToListAsync();
        }

        public async Task<Ingredients?> GetIngredientsByIdAsync(int id)
        {
            var ingredients = await _dbContext.Ingredientss.FirstOrDefaultAsync(ing => ing.IngredientId == id);

            if (ingredients == null)
                return null;

            return ingredients;
        }

        public async Task<Ingredients?> UpdateIngredientsAsync(int id, UpdateIngredientsDTO updateIngredientsDTO)
        {
            var ingredients = await _dbContext.Ingredientss.FirstOrDefaultAsync(ing => ing.IngredientId == id);

            if (ingredients == null)
                return null;

            ingredients.Name = updateIngredientsDTO.Name;
            ingredients.Description = updateIngredientsDTO.Description;
            ingredients.StoreId = updateIngredientsDTO.StoreId;
            ingredients.MinimumStockLevel = updateIngredientsDTO.MinimumStockLevel;
            ingredients.UnitOfMeasure = updateIngredientsDTO.UnitOfMeasure;
            ingredients.CostPerUnit = updateIngredientsDTO.CostPerUnit;
            ingredients.CaloriesPerUnit = updateIngredientsDTO.CaloriesPerUnit;
            ingredients.ProteinPerUnit = updateIngredientsDTO.ProteinPerUnit;
            ingredients.CarbohydratesPerUnit = updateIngredientsDTO.CarbohydratesPerUnit;
            ingredients.FatsPerUnit = updateIngredientsDTO.FatsPerUnit;
            ingredients.Allergens = updateIngredientsDTO.Allergens;
            ingredients.IsVegetarian = updateIngredientsDTO.IsVegetarian;
            ingredients.IsVegan = updateIngredientsDTO.IsVegan;
            ingredients.IsGlutenFree = updateIngredientsDTO.IsGlutenFree;
            ingredients.SupplierName = updateIngredientsDTO.SupplierName;
            ingredients.SupplierCode = updateIngredientsDTO.SupplierCode;
            ingredients.Status = ingredients.Status;
            ingredients.UpdatedAt = ingredients.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return ingredients;
        }
    }
}
