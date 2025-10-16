using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Recipes;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class RecipesController(IRecipes recipeRepo) : ControllerBase
    {
        private readonly IRecipes _recipes = recipeRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllRecipes()
        {
            try
            {
                var result = await _recipes.GetAllRecipesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            try
            {
                var result = await _recipes.GetRecipesByIdAsync(id);
                if (result == null)
                    return NotFound($"Recipe with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] Recipes recipes)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _recipes.CreateRecipesAsync(recipes);
                return CreatedAtAction(nameof(GetRecipeById), new { id = result?.RecipeId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromBody] UpdateRecipesDTO updateRecipesDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _recipes.UpdateRecipesByIdAsync(id, updateRecipesDTO);
                if (result == null)
                    return NotFound($"Recipe with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            try
            {
                var result = await _recipes.DeleteRecipesByIdAsync(id);
                if (result == null)
                    return NotFound($"Recipe with ID {id} not found.");
                return Ok($"Recipe with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
