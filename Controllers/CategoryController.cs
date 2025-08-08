using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ApiDbContext dbContext) : ControllerBase
    {
        private readonly ApiDbContext dbContext = dbContext;

        [HttpGet]
        public IActionResult GetAllCategories([FromQuery] string? name)
        {
            var query = dbContext.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.CategoryName!.Contains(name));

            return Ok(query.ToList());
        }

      
        [HttpGet("{id}")]
        public IActionResult GetCategoryById(int id)
        {
            var category = dbContext.Categories.Find(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

    
        [HttpPost]
        public IActionResult CreateCategory([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }

      
        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id, [FromBody] Category updatedCategory)
        {
            if (id != updatedCategory.CategoryId)
                return BadRequest("Category ID mismatch.");

            var category = dbContext.Categories.Find(id);
            if (category == null) return NotFound();

            dbContext.Entry(category).CurrentValues.SetValues(updatedCategory);
            dbContext.SaveChanges();
            return NoContent();
        }

      
        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            var category = dbContext.Categories.Find(id);
            if (category == null) return NotFound();

            dbContext.Categories.Remove(category);
            dbContext.SaveChanges();
            return NoContent();
        }
    }
}
