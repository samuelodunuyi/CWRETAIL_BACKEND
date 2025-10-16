using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using CWSERVER.Models.Industries.Restaurant.DTOs.Categories;
using CWSERVER.Data;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class CategoriesController(ICategories categoryRepo, ApiDbContext dbContext) : ControllerBase
    {
        private readonly ICategories _categories = categoryRepo;
        private readonly ApiDbContext _dbContext = dbContext;

        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] int? storeId, [FromQuery] string? name, [FromQuery] bool? isActive)
        {
            try
            {
                var query = _dbContext.Categoriess.AsQueryable();

                // Filter by StoreId if provided
                if (storeId.HasValue)
                    query = query.Where(c => c.StoreId == storeId.Value);

                // Filter by name if provided
                if (!string.IsNullOrEmpty(name))
                    query = query.Where(c => c.Name!.Contains(name));

                // Filter by active status if provided
                if (isActive.HasValue)
                    query = query.Where(c => c.IsActive == isActive.Value);

                var result = await query.ToListAsync();
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
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoriesDTO createCategoriesDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                // Validate StoreId if provided
                if (createCategoriesDTO.StoreId.HasValue)
                {
                    var storeExists = await _dbContext.Storess.AnyAsync(s => s.StoresId == createCategoriesDTO.StoreId.Value);
                    if (!storeExists)
                        return BadRequest($"Store with ID {createCategoriesDTO.StoreId.Value} does not exist.");
                }

                // Map DTO to entity
                var categories = new Categories
                {
                    Name = createCategoriesDTO.Name,
                    Description = createCategoriesDTO.Description,
                    StoreId = createCategoriesDTO.StoreId,
                    DisplayOrder = createCategoriesDTO.DisplayOrder,
                    IsActive = createCategoriesDTO.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

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
