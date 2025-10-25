using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories([FromQuery] int? storeId = null, [FromQuery] bool? isActive = null)
        {
            var query = _context.Categories.AsQueryable();

            // Determine authentication status
            var isAuthenticated = User?.Identity?.IsAuthenticated == true;

            // Apply filters
            if (storeId.HasValue)
            {
                query = query.Where(c => c.StoreId == storeId);
            }
            
            if (isAuthenticated)
            {
                // Authenticated users can control the isActive filter explicitly
                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }
            }
            else
            {
                // Anonymous users should only see active categories
                query = query.Where(c => c.IsActive);
            }

            // Order by DisplayOrder
            query = query.OrderBy(c => c.DisplayOrder);

            var categories = await query
                .Select(c => new CategoryPublicDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CategoryIcon = c.CategoryIcon,
                    StoreId = c.StoreId,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive
                })
                .ToListAsync();
            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(int id)
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated == true;

            var query = _context.Categories.Where(c => c.CategoryId == id);
            if (!isAuthenticated)
            {
                // Anonymous users should only see active categories
                query = query.Where(c => c.IsActive);
            }

            var category = await query.FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            var dto = new CategoryPublicDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CategoryIcon = category.CategoryIcon,
                StoreId = category.StoreId,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return Ok(dto);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "0")
            {
                // SuperAdmin can create any category
                // Validate that the store exists if StoreId is provided
                if (category.StoreId.HasValue)
                {
                    bool storeExists = await _context.Stores.AnyAsync(s => s.StoreId == category.StoreId.Value);
                    if (!storeExists)
                    {
                        return BadRequest(new { Message = $"Store with ID {category.StoreId.Value} does not exist" });
                    }
                }
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only create categories for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null)
                {
                    return Forbid();
                }

                // Ensure the category is assigned to the StoreAdmin's store
                if (category.StoreId.HasValue && category.StoreId.Value != store.StoreId)
                {
                    return BadRequest(new { Message = "You can only create categories for your store" });
                }

                category.StoreId = store.StoreId;
            }
            else
            {
                return Forbid();
            }

            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuditAsync("Create Category", $"Created category: {category.CategoryName}");

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, category);
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest();
            }

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can update any category
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only update categories for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || (existingCategory.StoreId.HasValue && existingCategory.StoreId.Value != store.StoreId))
                {
                    return Forbid();
                }

                // Ensure the category stays assigned to the StoreAdmin's store
                if (category.StoreId.HasValue && category.StoreId.Value != store.StoreId)
                {
                    return BadRequest(new { Message = "You can only assign categories to your store" });
                }

                category.StoreId = existingCategory.StoreId;
            }
            else
            {
                return Forbid();
            }

            // Update fields
            // Name field removed
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;
            existingCategory.CategoryIcon = category.CategoryIcon;
            existingCategory.DisplayOrder = category.DisplayOrder;
            existingCategory.IsActive = category.IsActive;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            // Only SuperAdmin can change store assignment
            if (currentUserRole == "SuperAdmin")
            {
                // Validate that the store exists if StoreId is provided
                if (category.StoreId.HasValue)
                {
                    bool storeExists = await _context.Stores.AnyAsync(s => s.StoreId == category.StoreId.Value);
                    if (!storeExists)
                    {
                        return BadRequest(new { Message = $"Store with ID {category.StoreId.Value} does not exist" });
                    }
                    existingCategory.StoreId = category.StoreId;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogAuditAsync("Update Category", $"Updated category: {category.CategoryName}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Category/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchCategory(int id, [FromBody] Category patchCategory)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can update any category
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only update categories for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || (category.StoreId.HasValue && category.StoreId.Value != store.StoreId))
                {
                    return Forbid();
                }

                // Ensure the category stays assigned to the StoreAdmin's store
                if (patchCategory.StoreId.HasValue && patchCategory.StoreId.Value != store.StoreId)
                {
                    return BadRequest(new { Message = "You can only assign categories to your store" });
                }

                // StoreAdmin cannot change store assignment
                patchCategory.StoreId = null;
            }
            else
            {
                return Forbid();
            }

            // Update non-null fields
            // Name field removed
            if (patchCategory.CategoryName != null)
                category.CategoryName = patchCategory.CategoryName;
            if (patchCategory.Description != null)
                category.Description = patchCategory.Description;
            if (patchCategory.CategoryIcon != null)
                category.CategoryIcon = patchCategory.CategoryIcon;
            if (patchCategory.DisplayOrder != 0)
                category.DisplayOrder = patchCategory.DisplayOrder;
            
            // IsActive is a boolean, so we need to check if it was included in the request
            if (patchCategory.IsActive != category.IsActive)
                category.IsActive = patchCategory.IsActive;
            
            // Only SuperAdmin can change store assignment
            if (currentUserRole == UserRole.SuperAdmin.ToString() && patchCategory.StoreId.HasValue)
            {
                // Validate that the store exists
                bool storeExists = await _context.Stores.AnyAsync(s => s.StoreId == patchCategory.StoreId.Value);
                if (!storeExists)
                {
                    return BadRequest(new { Message = $"Store with ID {patchCategory.StoreId.Value} does not exist" });
                }
                category.StoreId = patchCategory.StoreId;
            }
            
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Log the action
            await LogAuditAsync("Patch Category", $"Updated category: {category.CategoryName}");

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can delete any category
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only delete categories for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || (category.StoreId.HasValue && category.StoreId.Value != store.StoreId))
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            
            // Log the action
            await LogAuditAsync("Delete Category", $"Deleted category: {category.CategoryName}");

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        private async Task LogAuditAsync(string action, string details)
        {
            var auditLog = new AuditLog
            {
                Userid = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Action = action,
                Details = details,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
