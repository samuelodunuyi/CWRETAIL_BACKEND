using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using CWSERVER.Models.Industries.Restaurant.DTOs.Categories;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(ICategories categoryRepo) : ControllerBase
    {
        private readonly ICategories _categories = categoryRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var result = await _categories.GetAllCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var result = await _categories.GetCategoriesByIdAsync(id);
                if (result == null)
                    return NotFound($"Category with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Categories categories)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _categories.CreateCategoriesAsync(categories);
                return CreatedAtAction(nameof(GetCategoryById), new { id = result?.CategoryId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoriesDTO updateCategoriesDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _categories.UpdateCategoriesAsync(id, updateCategoriesDTO);
                if (result == null)
                    return NotFound($"Category with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _categories.DeleteCategoriesAsync(id);
                if (result == null)
                    return NotFound($"Category with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
