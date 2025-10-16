using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Ingredients;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CWSERVER.Data;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class IngredientsController(IIngredients ingredientsRepo, ApiDbContext dbContext) : ControllerBase
    {
        private readonly IIngredients _ingredients = ingredientsRepo;
        private readonly ApiDbContext _dbContext = dbContext;

        [HttpGet]
        public async Task<IActionResult> GetAllIngredients()
        {
            try
            {
                var result = await _ingredients.GetAllIngredientsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIngredientById(int id)
        {
            try
            {
                var result = await _ingredients.GetIngredientsByIdAsync(id);
                if (result == null)
                    return NotFound($"Ingredient with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateIngredient([FromBody] CreateIngredientsDTO createIngredientsDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                // Validate StoreId exists
                var storeExists = await _dbContext.Storess.AnyAsync(s => s.StoresId == createIngredientsDto.StoreId);
                if (!storeExists)
                {
                    return BadRequest($"Store with ID {createIngredientsDto.StoreId} does not exist.");
                }

                // Map DTO to entity
                var ingredients = new Ingredients
                {
                    Name = createIngredientsDto.Name,
                    Description = createIngredientsDto.Description,
                    StoreId = createIngredientsDto.StoreId,
                    MinimumStockLevel = createIngredientsDto.MinimumStockLevel,
                    UnitOfMeasure = createIngredientsDto.UnitOfMeasure,
                    CostPerUnit = createIngredientsDto.CostPerUnit,
                    CaloriesPerUnit = createIngredientsDto.CaloriesPerUnit,
                    ProteinPerUnit = createIngredientsDto.ProteinPerUnit,
                    CarbohydratesPerUnit = createIngredientsDto.CarbohydratesPerUnit,
                    FatsPerUnit = createIngredientsDto.FatsPerUnit,
                    Allergens = createIngredientsDto.Allergens,
                    IsVegetarian = createIngredientsDto.IsVegetarian,
                    IsVegan = createIngredientsDto.IsVegan,
                    IsGlutenFree = createIngredientsDto.IsGlutenFree,
                    SupplierName = createIngredientsDto.SupplierName,
                    SupplierCode = createIngredientsDto.SupplierCode,
                    Status = createIngredientsDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _ingredients.CreateIngredientsAsync(ingredients);
                return CreatedAtAction(nameof(GetIngredientById), new { id = result?.IngredientId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] UpdateIngredientsDTO updateIngredientsDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _ingredients.UpdateIngredientsAsync(id, updateIngredientsDTO);
                if (result == null)
                    return NotFound($"Ingredient with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            try
            {
                var result = await _ingredients.DeleteIngredientsAsync(id);
                if (result == null)
                    return NotFound($"Ingredient with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
