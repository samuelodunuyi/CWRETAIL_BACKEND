using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using CWSERVER.Models.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers.Core
{
    [Route("api/core/[controller]")]
    [ApiController]
    public class CategoryController(ApiDbContext dbContext) : ControllerBase
    {
        private readonly ApiDbContext dbContext = dbContext;

        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] string? name)
        {
            var query = dbContext.Categories.Include(c => c.Store).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.CategoryName!.Contains(name));

            var categories = await query.ToListAsync();
            var categoryDtos = categories.Select(c => new CategoryResponseDTO
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryIcon = c.CategoryIcon,
                StoreId = c.StoreId,
                StoreName = c.Store?.StoreName,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy
            }).ToList();

            return Ok(categoryDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await dbContext.Categories
                .Include(c => c.Store)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
                
            if (category == null) return NotFound();

            var categoryDto = new CategoryResponseDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryIcon = category.CategoryIcon,
                StoreId = category.StoreId,
                StoreName = category.Store?.StoreName,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.CreatedBy,
                UpdatedBy = category.UpdatedBy
            };

            return Ok(categoryDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate StoreId if provided
            if (categoryDto.StoreId.HasValue)
            {
                var storeExists = await dbContext.Stores.AnyAsync(s => s.StoreId == categoryDto.StoreId.Value);
                if (!storeExists)
                    return BadRequest($"Store with ID {categoryDto.StoreId.Value} does not exist.");
            }

            var category = new Category
            {
                CategoryName = categoryDto.CategoryName,
                CategoryIcon = categoryDto.CategoryIcon,
                StoreId = categoryDto.StoreId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System" // TODO: Get from authenticated user
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, 
                new CategoryResponseDTO
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    CategoryIcon = category.CategoryIcon,
                    StoreId = category.StoreId,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                    CreatedBy = category.CreatedBy,
                    UpdatedBy = category.UpdatedBy
                });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDTO categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await dbContext.Categories.Include(c => c.Store).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();

            // Validate StoreId if provided
            if (categoryDto.StoreId.HasValue)
            {
                var storeExists = await dbContext.Stores.AnyAsync(s => s.StoreId == categoryDto.StoreId.Value);
                if (!storeExists)
                    return BadRequest($"Store with ID {categoryDto.StoreId.Value} does not exist.");
            }

            category.CategoryName = categoryDto.CategoryName;
            category.CategoryIcon = categoryDto.CategoryIcon;
            category.StoreId = categoryDto.StoreId;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = "System"; // TODO: Get from authenticated user

            await dbContext.SaveChangesAsync();

            var result = new CategoryResponseDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryIcon = category.CategoryIcon,
                StoreId = category.StoreId,
                StoreName = category.Store?.StoreName,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.CreatedBy,
                UpdatedBy = category.UpdatedBy
            };

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await dbContext.Categories.FindAsync(id);
            if (category == null) return NotFound();

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
