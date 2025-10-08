using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] bool? isActive)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check if user is requesting inactive products
            bool showInactive = isActive.HasValue && isActive.Value == false;

            // Only allow SuperAdmin and StoreAdmin to see inactive products
            if (showInactive && (
                currentUserRole != "SuperAdmin" &&
                currentUserRole != "StoreAdmin"))
            {   
                return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
            }

            IQueryable<Product> query = _context.Products;

            // Filter by active status - default to active only unless specified
            if (!isActive.HasValue || isActive.Value)
            {
                query = query.Where(p => p.IsActive);
            }

            // Apply role-based filtering
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can see all products
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only see products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null)
                {
                    return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
                }

                query = query.Where(p => p.StoreId == store.StoreId);
            }
            else if (currentUserRole == "Employee")
            {
                // Employee can only see products for their assigned store
                var employee = await _context.Employees
                    .Include(e => e.Store)
                    .FirstOrDefaultAsync(e => e.User.Email == currentUserEmail);

                if (employee == null || employee.Store == null)
                {
                    return NotFound(new { Message = "Employee not assigned to any store" });
                }

                query = query.Where(p => p.StoreId == employee.StoreId);
            }
            else
            {
                // Customers can only see active products
                query = query.Where(p => p.IsActive && p.ShowInWeb);
            }

            bool isAdmin = currentUserRole == "SuperAdmin" ||
                         currentUserRole == "StoreAdmin";
            
            var products = await query
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.AdditionalImages)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    SKU = p.SKU,
                    Barcode = p.Barcode,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    StoreId = p.StoreId,
                    BasePrice = p.BasePrice,
                    CurrentStock = p.CurrentStock,
                    UnitOfMeasure = p.UnitOfMeasure,
                    ImageUrl = p.ImageUrl, // Keep this for backward compatibility
                    AdditionalImages = p.AdditionalImages.ToList(),
                    ShowInWeb = p.ShowInWeb,
                    ShowInPOS = p.ShowInPOS,
                    IsActive = p.IsActive,
                    // Conditional store information based on user role
                    Store = isAdmin 
                        ? new StoreFullInfoDTO 
                        {
                            StoreId = p.Store.StoreId,
                            StoreName = p.Store.StoreName,
                            StorePhoneNumber = p.Store.StorePhoneNumber,
                            StoreEmailAddress = p.Store.StoreEmailAddress,
                            StoreAddress = p.Store.StoreAddress,
                            StoreAdminId = null, // This can be updated if needed
                            StoreType = p.Store.StoreType,
                            IsActive = p.Store.IsActive,
                            CreatedAt = p.Store.CreatedAt,
                            UpdatedAt = p.Store.UpdatedAt
                        }
                        : new StoreBasicInfoDTO
                        {
                            StoreId = p.Store.StoreId,
                            StoreName = p.Store.StoreName,
                            StoreType = p.Store.StoreType,
                            IsActive = p.Store.IsActive
                        },
                    // Only include user info for admins
                    CreatedByUser = isAdmin && p.CreatedBy != null
                        ? new UserBasicInfoDTO 
                        {
                            Id = _context.Users.Where(u => u.Email == p.CreatedBy).Select(u => u.Id).FirstOrDefault(),
                            Username = _context.Users.Where(u => u.Email == p.CreatedBy).Select(u => u.Username).FirstOrDefault() ?? string.Empty,
                            Email = p.CreatedBy,
                            FirstName = _context.Users.Where(u => u.Email == p.CreatedBy).Select(u => u.FirstName).FirstOrDefault() ?? string.Empty,
                            LastName = _context.Users.Where(u => u.Email == p.CreatedBy).Select(u => u.LastName).FirstOrDefault() ?? string.Empty,
                            RoleName = _context.Users.Where(u => u.Email == p.CreatedBy).Select(u => u.Role.Name).FirstOrDefault() ?? string.Empty
                        }
                        : null
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            bool isAdmin = currentUserRole == "SuperAdmin" ||
                currentUserRole == "StoreAdmin";
            
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if product is inactive and user has permission to view it
            if (!product.IsActive && 
                currentUserRole != UserRole.SuperAdmin.ToString() && 
                currentUserRole != UserRole.StoreAdmin.ToString())
            {
                return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
            }

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can access any product
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only access products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || product.StoreId != store.StoreId)
                {
                    return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
                }
            }
            else if (currentUserRole == "Employee")
            {
                // Employee can only access products for their assigned store
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.User.Email == currentUserEmail);

                if (employee == null || product.StoreId != employee.StoreId)
                {
                    return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
                }
            }
            else
            {
                // Customers can only see active products
                if (!product.IsActive || !product.ShowInWeb)
                {
                    return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
                }
            }

            var productDto = new ProductDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                SKU = product.SKU,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                StoreId = product.StoreId,
                BasePrice = product.BasePrice,
                CurrentStock = product.CurrentStock,
                UnitOfMeasure = product.UnitOfMeasure,
                ImageUrl = product.ImageUrl,
                AdditionalImages = product.AdditionalImages.ToList(),
                ShowInWeb = product.ShowInWeb,
                ShowInPOS = product.ShowInPOS,
                IsActive = product.IsActive,
                // Conditional store information based on user role
                Store = product.Store != null ? (isAdmin 
                    ? new StoreFullInfoDTO 
                    {
                        StoreId = product.Store.StoreId,
                        StoreName = product.Store.StoreName,
                        StorePhoneNumber = product.Store.StorePhoneNumber ?? string.Empty,
                        StoreEmailAddress = product.Store.StoreEmailAddress ?? string.Empty,
                        StoreAddress = product.Store.StoreAddress ?? string.Empty,
                        StoreAdminId = null, // This can be updated if needed
                        StoreType = product.Store.StoreType ?? string.Empty,
                        IsActive = product.Store.IsActive,
                        CreatedAt = product.Store.CreatedAt,
                        UpdatedAt = product.Store.UpdatedAt
                    }
                    : (object)new StoreBasicInfoDTO
                    {
                        StoreId = product.Store.StoreId,
                        StoreName = product.Store.StoreName ?? string.Empty,
                        StoreType = product.Store.StoreType ?? string.Empty,
                        IsActive = product.Store.IsActive
                    }) : null,
                // Only include user info for admins
                CreatedByUser = isAdmin && !string.IsNullOrEmpty(product.CreatedBy)
                    ? new UserBasicInfoDTO 
                    {
                        Id = _context.Users.FirstOrDefault(u => u.Email == product.CreatedBy)?.Id ?? 0,
                        Username = _context.Users.FirstOrDefault(u => u.Email == product.CreatedBy)?.Username ?? string.Empty,
                        Email = product.CreatedBy,
                        FirstName = _context.Users.FirstOrDefault(u => u.Email == product.CreatedBy)?.FirstName ?? string.Empty,
                        LastName = _context.Users.FirstOrDefault(u => u.Email == product.CreatedBy)?.LastName ?? string.Empty,
                        RoleName = _context.Users.FirstOrDefault(u => u.Email == product.CreatedBy)?.Role?.Name ?? string.Empty
                    }
                    : null
            };

            return Ok(productDto);
        }

        // GET: api/Product/test
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult TestEndpoint()
        {
            return Ok(new { Message = "Test endpoint works!" });
        }

        // POST: api/Product
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Validate that the store exists
            bool storeExists = await _context.Stores.AnyAsync(s => s.StoreId == product.StoreId);
            if (!storeExists)
            {
                return BadRequest(new { Message = $"Store with ID {product.StoreId} does not exist" });
            }

            // Validate that the category exists
            bool categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == product.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { Message = $"Category with ID {product.CategoryId} does not exist" });
            }

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can create any product
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only create products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null)
                {
                    return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
                }

                // Ensure the product is assigned to the StoreAdmin's store
                if (product.StoreId != store.StoreId)
                {
                    return BadRequest(new { Message = "You can only create products for your store" });
                }
            }
            else
            {
                return StatusCode(403, new { message = $"Access denied. User role: {User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No role found"}" });
            }

            product.CreatedAt = DateTime.UtcNow;
            product.CreatedBy = currentUserEmail;
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuditAsync("Create Product", $"Created product: {product.ProductName}");

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Validate that the store exists
            bool storeExists = await _context.Stores.AnyAsync(s => s.StoreId == product.StoreId);
            if (!storeExists)
            {
                return BadRequest(new { Message = $"Store with ID {product.StoreId} does not exist" });
            }

            // Validate that the category exists
            bool categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == product.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { Message = $"Category with ID {product.CategoryId} does not exist" });
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can update any product
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only update products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || existingProduct.StoreId != store.StoreId)
                {
                    return Forbid();
                }

                // Ensure the product stays assigned to the StoreAdmin's store
                if (product.StoreId != store.StoreId)
                {
                    return BadRequest(new { Message = "You can only assign products to your store" });
                }
            }
            else
            {
                return Forbid();
            }

            // Update fields
            existingProduct.ProductName = product.ProductName;
            existingProduct.Description = product.Description;
            existingProduct.SKU = product.SKU;
            existingProduct.Barcode = product.Barcode;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BasePrice = product.BasePrice;
            existingProduct.CostPrice = product.CostPrice;
            // CurrentStock should not be editable via PUT
            existingProduct.MinimumStockLevel = product.MinimumStockLevel;
            existingProduct.MaximumStockLevel = product.MaximumStockLevel;
            existingProduct.UnitOfMeasure = product.UnitOfMeasure;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.ShowInWeb = product.ShowInWeb;
            existingProduct.ShowInPOS = product.ShowInPOS;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;
            existingProduct.UpdatedBy = currentUserEmail;

            // Only SuperAdmin can change store assignment
            if (currentUserRole == "SuperAdmin")
            {
                existingProduct.StoreId = product.StoreId;
            }

            try
            {
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogAuditAsync("Update Product", $"Updated product: {product.ProductName}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Return the updated product
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return Ok(updatedProduct);
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can delete any product
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only delete products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || product.StoreId != store.StoreId)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            // Log the action
            await LogAuditAsync("Delete Product", $"Deleted product: {product.ProductName}");

            return NoContent();
        }

        // PATCH: api/Product/5
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PatchProduct(int id, [FromBody] Dictionary<string, object> patchDoc)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can update any product
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin can only update products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || product.StoreId != store.StoreId)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            // Apply patch operations
            foreach (var item in patchDoc)
            {
                // Skip CurrentStock - it should only be updated via Restock endpoint
                if (item.Key.Equals("currentStock", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Handle StoreId specially - only SuperAdmin can change it
                if (item.Key.Equals("storeId", StringComparison.OrdinalIgnoreCase))
                {
                    if (currentUserRole == UserRole.SuperAdmin.ToString())
                    {
                        if (int.TryParse(item.Value.ToString(), out int storeId))
                        {
                            bool storeExists = await _context.Stores.AnyAsync(s => s.StoreId == storeId);
                            if (!storeExists)
                            {
                                return BadRequest(new { Message = $"Store with ID {storeId} does not exist" });
                            }
                            product.StoreId = storeId;
                        }
                    }
                    continue;
                }

                // Handle CategoryId specially - validate it exists
                if (item.Key.Equals("categoryId", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(item.Value.ToString(), out int categoryId))
                    {
                        bool categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
                        if (!categoryExists)
                        {
                            return BadRequest(new { Message = $"Category with ID {categoryId} does not exist" });
                        }
                        product.CategoryId = categoryId;
                    }
                    continue;
                }

                // Apply other properties
                switch (item.Key.ToLower())
                {
                    case "productname":
                        product.ProductName = item.Value.ToString();
                        break;
                    case "description":
                        product.Description = item.Value.ToString();
                        break;
                    case "sku":
                        product.SKU = item.Value.ToString();
                        break;
                    case "barcode":
                        product.Barcode = item.Value.ToString();
                        break;
                    case "baseprice":
                        if (decimal.TryParse(item.Value.ToString(), out decimal basePrice))
                            product.BasePrice = basePrice;
                        break;
                    case "costprice":
                        if (decimal.TryParse(item.Value.ToString(), out decimal costPrice))
                            product.CostPrice = costPrice;
                        break;
                    case "minimumstocklevel":
                        if (int.TryParse(item.Value.ToString(), out int minStock))
                            product.MinimumStockLevel = minStock;
                        break;
                    case "maximumstocklevel":
                        if (int.TryParse(item.Value.ToString(), out int maxStock))
                            product.MaximumStockLevel = maxStock;
                        break;
                    case "unitofmeasure":
                        product.UnitOfMeasure = item.Value.ToString();
                        break;
                    case "imageurl":
                    product.ImageUrl = item.Value.ToString();
                        break;
                    case "showinweb":
                        if (bool.TryParse(item.Value.ToString(), out bool showInWeb))
                            product.ShowInWeb = showInWeb;
                        break;
                    case "showinpos":
                        if (bool.TryParse(item.Value.ToString(), out bool showInPOS))
                            product.ShowInPOS = showInPOS;
                        break;
                    case "isactive":
                        if (bool.TryParse(item.Value.ToString(), out bool isActive))
                            product.IsActive = isActive;
                        break;
                    case "additionalimages":
                        // Handle additional images
                        if (item.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                        {
                            // Clear existing additional images
                            var existingImages = await _context.ProductImages
                                .Where(pi => pi.ProductId == product.ProductId)
                                .ToListAsync();
                            
                            _context.ProductImages.RemoveRange(existingImages);
                            
                            // Add new images
                            foreach (var imageElement in jsonElement.EnumerateArray())
                            {
                                var productImage = new ProductImage
                                  {
                                      ProductId = product.ProductId,
                                      ImagePath = imageElement.GetString()
                                  };
                                _context.ProductImages.Add(productImage);
                            }
                        }
                        break;
                }
            }

            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = currentUserEmail;

            try
            {
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogAuditAsync("Patch Product", $"Updated product: {product.ProductName}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Return the updated product
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return Ok(updatedProduct);
        }

        // POST: api/Product/Restock
        [HttpPost("Restock")]
        [Authorize]
        public async Task<IActionResult> RestockProduct([FromBody] RestockRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest(new { Message = "Invalid restock request. ProductId and Quantity must be positive values." });
            }

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {request.ProductId} not found." });
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == UserRole.SuperAdmin.ToString())
            {
                // SuperAdmin can restock any product
            }
            else if (currentUserRole == UserRole.StoreAdmin.ToString())
            {
                // StoreAdmin can only restock products for their store
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (store == null || product.StoreId != store.StoreId)
                {
                    return Forbid();
                }
            }
            else if (currentUserRole == "Employee")
            {
                // Employee can only restock products for their assigned store
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.User.Email == currentUserEmail);

                if (employee == null || product.StoreId != employee.StoreId)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            // Update stock
            int previousStock = product.CurrentStock;
            product.CurrentStock += request.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = currentUserEmail;

            await _context.SaveChangesAsync();
            
            // Log the action
            await LogAuditAsync("Restock Product", 
                $"Restocked product: {product.ProductName}, Added: {request.Quantity}, Previous: {previousStock}, New: {product.CurrentStock}");

            return Ok(new { 
                Message = "Product restocked successfully", 
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                PreviousStock = previousStock,
                AddedStock = request.Quantity,
                CurrentStock = product.CurrentStock
            });
        }

        // Helper class for restock request
        public class RestockRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private async Task LogAuditAsync(string action, string details)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                Userid = userId,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
