using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Ingredients;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController(IIngredients ingredientsRepo) : ControllerBase
    {
        private readonly IIngredients _ingredients = ingredientsRepo;

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
        public async Task<IActionResult> CreateIngredient([FromBody] Ingredients ingredients)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
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
