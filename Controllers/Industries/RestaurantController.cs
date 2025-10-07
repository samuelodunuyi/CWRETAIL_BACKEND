using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using CW_RETAIL.Models.Industries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CW_RETAIL.Controllers.Industries
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RestaurantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Restaurant
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantProductDTO>>> GetRestaurantProducts()
        {
            var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("StoreAdmin");
            
            var products = await _context.RestaurantProducts
                .Include(p => p.Store)
                .Where(p => p.IsActive)
                .ToListAsync();
                
            var productDtos = products.Select(p => new RestaurantProductDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                SKU = p.SKU,
                Barcode = p.Barcode,
                CategoryId = p.CategoryId,
                StoreId = p.StoreId,
                BasePrice = p.BasePrice,
                CurrentStock = p.CurrentStock,
                UnitOfMeasure = p.UnitOfMeasure,
                ImageUrl = p.ImageUrl,
                ShowInWeb = p.ShowInWeb,
                ShowInPOS = p.ShowInPOS,
                IsActive = p.IsActive,
                IsRecipe = p.IsRecipe,
                PrepTimeMinutes = p.PrepTimeMinutes,
                CookingTimeMinutes = p.CookingTimeMinutes,
                Allergens = p.Allergens,
                SpiceLevel = p.SpiceLevel,
                DietTypes = p.DietTypes,
                Store = isAdmin 
                    ? new CW_RETAIL.Models.Industries.StoreFullInfoDTO 
                    {
                        StoreId = p.Store.StoreId,
                        StoreName = p.Store.StoreName,
                        StoreAddress = p.Store.StoreAddress,
                        StorePhoneNumber = p.Store.StorePhoneNumber,
                        StoreEmailAddress = p.Store.StoreEmailAddress,
                        StoreAdmin = p.Store.StoreAdmin,
                        StoreType = p.Store.StoreType,
                        IsActive = p.Store.IsActive
                    }
                    : new CW_RETAIL.Models.Industries.StoreBasicInfoDTO
                    {
                        StoreId = p.Store.StoreId,
                        StoreName = p.Store.StoreName,
                        StoreAddress = p.Store.StoreAddress
                    }
            }).ToList();
            
            return productDtos;
        }

        // GET: api/Restaurant/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantProductDTO>> GetRestaurantProduct(int id)
        {
            var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("StoreAdmin");
            
            var restaurantProduct = await _context.RestaurantProducts
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (restaurantProduct == null)
            {
                return NotFound();
            }

            var productDto = new RestaurantProductDTO
            {
                ProductId = restaurantProduct.ProductId,
                ProductName = restaurantProduct.ProductName,
                Description = restaurantProduct.Description,
                SKU = restaurantProduct.SKU,
                Barcode = restaurantProduct.Barcode,
                CategoryId = restaurantProduct.CategoryId,
                StoreId = restaurantProduct.StoreId,
                BasePrice = restaurantProduct.BasePrice,
                CurrentStock = restaurantProduct.CurrentStock,
                UnitOfMeasure = restaurantProduct.UnitOfMeasure,
                ImageUrl = restaurantProduct.ImageUrl,
                ShowInWeb = restaurantProduct.ShowInWeb,
                ShowInPOS = restaurantProduct.ShowInPOS,
                IsActive = restaurantProduct.IsActive,
                IsRecipe = restaurantProduct.IsRecipe,
                PrepTimeMinutes = restaurantProduct.PrepTimeMinutes,
                CookingTimeMinutes = restaurantProduct.CookingTimeMinutes,
                Allergens = restaurantProduct.Allergens,
                SpiceLevel = restaurantProduct.SpiceLevel,
                DietTypes = restaurantProduct.DietTypes,
                Store = isAdmin 
                    ? new CW_RETAIL.Models.Industries.StoreFullInfoDTO 
                    {
                        StoreId = restaurantProduct.Store.StoreId,
                        StoreName = restaurantProduct.Store.StoreName,
                        StoreAddress = restaurantProduct.Store.StoreAddress,
                        StorePhoneNumber = restaurantProduct.Store.StorePhoneNumber,
                        StoreEmailAddress = restaurantProduct.Store.StoreEmailAddress,
                        StoreAdmin = restaurantProduct.Store.StoreAdmin,
                        StoreType = restaurantProduct.Store.StoreType,
                        IsActive = restaurantProduct.Store.IsActive
                    }
                    : new CW_RETAIL.Models.Industries.StoreBasicInfoDTO
                    {
                        StoreId = restaurantProduct.Store.StoreId,
                        StoreName = restaurantProduct.Store.StoreName,
                        StoreAddress = restaurantProduct.Store.StoreAddress
                    }
            };
            
            return productDto;
        }

        // GET: api/Restaurant/Recipes
        [HttpGet("Recipes")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .ToListAsync();
        }

        // GET: api/Restaurant/Recipes/5
        [HttpGet("Recipes/{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Product)
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return recipe;
        }

        // GET: api/Restaurant/Recipes/ByIngredient/5
        [HttpGet("Recipes/ByIngredient/{ingredientId}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipesByIngredient(int ingredientId)
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .Include(r => r.Ingredients)
                .Where(r => r.Ingredients.Any(i => i.IngredientId == ingredientId))
                .ToListAsync();
        }

        // POST: api/Restaurant
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<RestaurantProduct>> PostRestaurantProduct(RestaurantProduct restaurantProduct)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            restaurantProduct.CreatedBy = userEmail;
            restaurantProduct.CreatedAt = DateTime.UtcNow;
            restaurantProduct.UpdatedAt = DateTime.UtcNow;

            _context.RestaurantProducts.Add(restaurantProduct);
            await _context.SaveChangesAsync();

            // Add audit log
            var auditLog = new AuditLog
                {
                    Action = "Create",
                    Details = $"Created new restaurant product: {restaurantProduct.ProductName}",
                    Userid = User.FindFirstValue(ClaimTypes.Email),
                    CreatedAt = DateTime.UtcNow
                };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRestaurantProduct", new { id = restaurantProduct.ProductId }, restaurantProduct);
        }

        // POST: api/Restaurant/Recipes
        [HttpPost("Recipes")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<Recipe>> PostRecipe(Recipe recipe)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            recipe.CreatedBy = userId;
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.UpdatedAt = DateTime.UtcNow;

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            // Add audit log
            var auditLog = new AuditLog
            {
                Action = "Create",
                Details = $"Created new recipe: {recipe.Name}",
                Userid = User.FindFirstValue(ClaimTypes.Email),
                CreatedAt = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipe", new { id = recipe.RecipeId }, recipe);
        }

        // PUT: api/Restaurant/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> PutRestaurantProduct(int id, RestaurantProduct restaurantProduct)
        {
            if (id != restaurantProduct.ProductId)
            {
                return BadRequest();
            }

            var existingProduct = await _context.RestaurantProducts.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            restaurantProduct.UpdatedAt = DateTime.UtcNow;
            restaurantProduct.UpdatedBy = userEmail;

            _context.Entry(existingProduct).State = EntityState.Detached;
            _context.Entry(restaurantProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Add audit log
                var auditLog = new AuditLog
                {
                    Action = "Update",
                    Details = $"Updated restaurant product: {restaurantProduct.ProductName}",
                    Userid = userEmail,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantProductExists(id))
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

        // PUT: api/Restaurant/Recipes/5
        [HttpPut("Recipes/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> PutRecipe(int id, Recipe recipe)
        {
            if (id != recipe.RecipeId)
            {
                return BadRequest();
            }

            var existingRecipe = await _context.Recipes.FindAsync(id);
            if (existingRecipe == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            recipe.UpdatedAt = DateTime.UtcNow;
            recipe.CreatedBy = existingRecipe.CreatedBy; // Preserve original creator

            _context.Entry(existingRecipe).State = EntityState.Detached;
            _context.Entry(recipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Add audit log
                var auditLog = new AuditLog
                {
                    Action = "Update",
                    Details = $"Updated recipe: {recipe.Name}",
                    Userid = userEmail,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeExists(id))
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

        // DELETE: api/Restaurant/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteRestaurantProduct(int id)
        {
            var restaurantProduct = await _context.RestaurantProducts.FindAsync(id);
            if (restaurantProduct == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            // Soft delete
            restaurantProduct.IsActive = false;
            restaurantProduct.UpdatedAt = DateTime.UtcNow;
            restaurantProduct.UpdatedBy = userEmail;
            
            await _context.SaveChangesAsync();

            // Add audit log
            var auditLog = new AuditLog
                {
                    Action = "Delete",
                    Details = $"Deleted restaurant product: {restaurantProduct.ProductName}",
                    Userid = userEmail,
                    CreatedAt = DateTime.UtcNow
                };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Restaurant/Recipes/5
        [HttpDelete("Recipes/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            // Add audit log
            var auditLog = new AuditLog
            {
                Action = "Delete",
                Details = $"Deleted recipe: {recipe.Name}",
                Userid = userEmail,
                CreatedAt = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RestaurantProductExists(int id)
        {
            return _context.RestaurantProducts.Any(e => e.ProductId == id);
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.RecipeId == id);
        }
    }
}