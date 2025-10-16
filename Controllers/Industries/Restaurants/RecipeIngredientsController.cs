using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class RecipeIngredientsController(IRecipeIngredients recipeIngredientsRepo) : ControllerBase
    {
        private readonly IRecipeIngredients _recipeIngredients = recipeIngredientsRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllRecipeIngredients()
        {
            try
            {
                var result = await _recipeIngredients.GetAllRecipeIngredientsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeIngredientById(int id)
        {
            try
            {
                var result = await _recipeIngredients.GetRecipeIngredientsByIdAsync(id);
                if (result == null)
                    return NotFound($"Recipe Ingredient with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipeIngredient([FromBody] RecipeIngredients recipeIngredients)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _recipeIngredients.CreateRecipeIngredientsAsync(recipeIngredients);
                return CreatedAtAction(nameof(GetRecipeIngredientById), new { id = result?.RecipeIngredientsId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipeIngredient(int id)
        {
            try
            {
                var result = await _recipeIngredients.DeleteRecipeIngredientsAsync(id);
                if (result == null)
                    return NotFound($"Recipe Ingredient with ID {id} not found.");
                return Ok($"Recipe Ingredient with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
