using CWSERVER.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using CWSERVER.Models.Core.DTOs;
using CWSERVER.Models.Core.Entities;
using CWSERVER.Services;

namespace CWSERVER.Controllers.Core
{
    [Route("api/core/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApiDbContext dbContext;
    private readonly IWebHostEnvironment hostingEnvironment;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IUserContextService userContextService;

    public ProductController(
        ApiDbContext dbContext, 
        IWebHostEnvironment hostingEnvironment, 
        IHttpContextAccessor httpContextAccessor,
        IUserContextService userContextService)
    {
        this.dbContext = dbContext;
        this.hostingEnvironment = hostingEnvironment;
        this.httpContextAccessor = httpContextAccessor;
        this.userContextService = userContextService;
    }

        private string SaveImageAndGetPath(IFormFile imageFile, int productId)
        {
            var uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images", "products", productId.ToString());
            //var uploadsFolder = Path.Combine("D:\\home\\site\\wwwroot", "images", "products", productId.ToString());

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null.");
            var request = httpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var relativePath = Path.Combine("images", "products", productId.ToString(), uniqueFileName).Replace("\\", "/");

            return $"{baseUrl}/{relativePath}";
        }


        private async Task<ProductResponseDTO> MapProductAsync(Product product)
        {
            var full = await dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.AdditionalImages)
                .FirstAsync(p => p.ProductId == product.ProductId);

            return new ProductResponseDTO
            {
                ProductId = full.ProductId,
                ProductName = full.ProductName,
                CategoryId = full.CategoryId,
                CategoryName = full.Category?.CategoryName,
                StoreId = full.StoreId,
                StoreName = full.Store?.StoreName,
                MainImageUrl = full.MainImagePath,
                AdditionalImages = full.AdditionalImages
                    .Select(i => new ProductImageDTO
                    {
                        Id = i.Id,
                        ImageUrl = i.ImagePath ?? ""
                    })
                    .ToList(),
                ProductLabel = full.ProductLabel,
                ProductAmountInStock = full.ProductAmountInStock,
                ProductPrice = full.ProductPrice,
                ProductOriginalPrice = full.ProductOriginalPrice,
                ProductDescription = full.ProductDescription,
                ProductSKU = full.ProductSKU,
                LowStockWarningCount = full.LowStockWarningCount,
                Status = full.Status
            };
        }





        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int? storeId,
        [FromQuery] decimal? productPriceFrom,
        [FromQuery] decimal? productPriceTo,
        [FromQuery] string? productSKU,
        [FromQuery] string? active)
        {
            var query = dbContext.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.Store)
                                 .Include(p => p.AdditionalImages)
                                 .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (storeId.HasValue)
                query = query.Where(p => p.StoreId == storeId.Value);

            if (productPriceFrom.HasValue)
                query = query.Where(p => p.ProductPrice >= productPriceFrom.Value);

            if (productPriceTo.HasValue)
                query = query.Where(p => p.ProductPrice <= productPriceTo.Value);

            if (!string.IsNullOrEmpty(productSKU))
                query = query.Where(p => p.ProductSKU == productSKU);

            if (string.IsNullOrEmpty(active) || active.ToLower() == "true")
                query = query.Where(p => p.Status == true);
            else if (active.ToLower() == "false")
                query = query.Where(p => p.Status == false);

            var filteredProducts = query.ToList();

            var response = filteredProducts.Select(p => new ProductResponseDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                StoreId = p.StoreId,
                StoreName = p.Store?.StoreName,
                MainImageUrl = p.MainImagePath,
                AdditionalImages = p.AdditionalImages.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImagePath ?? ""
                }).ToList(),
                ProductLabel = p.ProductLabel,
                ProductAmountInStock = p.ProductAmountInStock,
                ProductPrice = p.ProductPrice,
                ProductOriginalPrice = p.ProductOriginalPrice,
                ProductDescription = p.ProductDescription,
                ProductSKU = p.ProductSKU,
                Status = p.Status,
                LowStockWarningCount = p.LowStockWarningCount,

            });

            return Ok(response);
        }



        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = dbContext.Products
                                   .Include(p => p.Category)
                                   .Include(p => p.Store)
                                   .Include(p => p.AdditionalImages)
                                   .FirstOrDefault(p => p.ProductId == id);

            if (product == null) return NotFound();

            var response = new ProductResponseDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                StoreId = product.StoreId,
                StoreName = product.Store?.StoreName,
                MainImageUrl = product.MainImagePath,
                AdditionalImages = product.AdditionalImages.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImagePath ?? ""
                }).ToList(),
                ProductLabel = product.ProductLabel,
                ProductAmountInStock = product.ProductAmountInStock,
                ProductPrice = product.ProductPrice,
                ProductOriginalPrice = product.ProductOriginalPrice,
                ProductDescription = product.ProductDescription,
                ProductSKU = product.ProductSKU,
                Status = product.Status,
                LowStockWarningCount = product.LowStockWarningCount,
            };

            return Ok(response);
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            [FromForm] ProductCreateDTO productDto,
            IFormFile? mainImage,
            List<IFormFile>? additionalImages)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validate foreign key references
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId);
                if (!categoryExists)
                    return BadRequest($"Category with ID {productDto.CategoryId} does not exist.");

                var storeExists = await dbContext.Stores.AnyAsync(s => s.StoreId == productDto.StoreId);
                if (!storeExists)
                    return BadRequest($"Store with ID {productDto.StoreId} does not exist.");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(productDto.ProductName))
                    return BadRequest("Product name is required");

                if (productDto.ProductPrice <= 0)
                    return BadRequest("Product price must be greater than zero");

                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    CategoryId = productDto.CategoryId,
                    StoreId = productDto.StoreId,
                    ProductLabel = productDto.ProductLabel,
                    ProductAmountInStock = 0, // Default to 0, use restock endpoint to add inventory
                    ProductPrice = productDto.ProductPrice,
                    ProductOriginalPrice = productDto.ProductOriginalPrice,
                    ProductDescription = productDto.ProductDescription ?? string.Empty,
                    ProductSKU = productDto.ProductSKU ?? string.Empty,
                    LowStockWarningCount = productDto.LowStockWarningCount,
                    Status = productDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userContextService.GetCurrentUsername()
                };

                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();

                if (mainImage != null)
                {
                    if (!mainImage.ContentType.StartsWith("image/"))
                        return BadRequest("Main image file must be an image");
                        
                    var imagePath = SaveImageAndGetPath(mainImage, product.ProductId);
                    product.MainImagePath = imagePath;
                }

                if (additionalImages != null && additionalImages.Count != 0)
                {
                    foreach (var image in additionalImages)
                    {
                        if (!image.ContentType.StartsWith("image/"))
                            return BadRequest("All additional image files must be images");
                            
                        var imagePath = SaveImageAndGetPath(image, product.ProductId);
                        product.AdditionalImages.Add(new ProductImage { ImagePath = imagePath });
                    }
                }

                await dbContext.SaveChangesAsync();

                var response = await MapProductAsync(product);
                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, "Database error occurred while saving product: " + dbEx.Message);
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, "File operation error: " + ioEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred: " + ex.Message);
            }
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(
        int id,
        [FromForm] ProductUpdateDTO productDto,
        IFormFile? mainImage,
        List<IFormFile>? additionalImages)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await dbContext.Products
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            // Validate foreign key references
            var categoryExists = await dbContext.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId);
            if (!categoryExists)
                return BadRequest($"Category with ID {productDto.CategoryId} does not exist.");

            var storeExists = await dbContext.Stores.AnyAsync(s => s.StoreId == productDto.StoreId);
            if (!storeExists)
                return BadRequest($"Store with ID {productDto.StoreId} does not exist.");

            product.ProductName = productDto.ProductName ?? "";
            product.CategoryId = productDto.CategoryId;
            product.StoreId = productDto.StoreId;
            product.ProductLabel = productDto.ProductLabel;
            // Note: ProductAmountInStock is not updated here, use restock endpoint
            product.ProductPrice = productDto.ProductPrice;
            product.ProductOriginalPrice = productDto.ProductOriginalPrice;
            product.ProductDescription = productDto.ProductDescription;
            product.ProductSKU = productDto.ProductSKU ?? "";
            product.LowStockWarningCount = productDto.LowStockWarningCount;
            product.Status = productDto.Status;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = "System"; // TODO: Get from authenticated user

            if (mainImage != null)
            {
                var imagePath = SaveImageAndGetPath(mainImage, product.ProductId);
                product.MainImagePath = imagePath;
            }

            // Replace all additional images
            if (product.AdditionalImages.Any())
            {
                dbContext.RemoveRange(product.AdditionalImages);
            }

            if (additionalImages != null && additionalImages.Any())
            {
                product.AdditionalImages = new List<ProductImage>();

                foreach (var image in additionalImages)
                {
                    var imagePath = SaveImageAndGetPath(image, product.ProductId);
                    product.AdditionalImages.Add(new ProductImage { ImagePath = imagePath });
                }
            }

            await dbContext.SaveChangesAsync();
            var result = await MapProductAsync(product);
            return Ok(result);

        }




        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProduct(
        int id,
        [FromForm] ProductUpdateDTO productDto,
        IFormFile? mainImage,
        List<IFormFile>? additionalImages)
        {
            var product = await dbContext.Products
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            // Validate foreign key references if they are being updated
            if (productDto.CategoryId != 0)
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId);
                if (!categoryExists)
                    return BadRequest($"Category with ID {productDto.CategoryId} does not exist.");
            }

            if (productDto.StoreId != 0)
            {
                var storeExists = await dbContext.Stores.AnyAsync(s => s.StoreId == productDto.StoreId);
                if (!storeExists)
                    return BadRequest($"Store with ID {productDto.StoreId} does not exist.");
            }

            if (!string.IsNullOrEmpty(productDto.ProductName)) product.ProductName = productDto.ProductName;
            if (productDto.CategoryId != 0) product.CategoryId = productDto.CategoryId;
            if (productDto.StoreId != 0) product.StoreId = productDto.StoreId;
            if (!string.IsNullOrEmpty(productDto.ProductLabel)) product.ProductLabel = productDto.ProductLabel;
            // Note: ProductAmountInStock is not updated here, use restock endpoint
            if (productDto.ProductPrice != 0) product.ProductPrice = productDto.ProductPrice;
            if (productDto.ProductOriginalPrice.HasValue) product.ProductOriginalPrice = productDto.ProductOriginalPrice;
            if (!string.IsNullOrEmpty(productDto.ProductDescription)) product.ProductDescription = productDto.ProductDescription;
            if (!string.IsNullOrEmpty(productDto.ProductSKU)) product.ProductSKU = productDto.ProductSKU;

            product.LowStockWarningCount = productDto.LowStockWarningCount;
            product.Status = productDto.Status;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = "System"; // TODO: Get from authenticated user

            if (mainImage != null)
            {
                var imagePath = SaveImageAndGetPath(mainImage, product.ProductId);
                product.MainImagePath = imagePath;
            }

            // Delete specific additional images
            if (productDto.DeleteImageIds != null && productDto.DeleteImageIds.Any())
            {
                var imagesToDelete = product.AdditionalImages
                    .Where(img => productDto.DeleteImageIds.Contains(img.Id))
                    .ToList();

                foreach (var image in imagesToDelete)
                {
                    if (!string.IsNullOrEmpty(image.ImagePath))
                    {
                        var fullPath = Path.Combine(hostingEnvironment.WebRootPath, image.ImagePath.Replace("/", "\\").Split("wwwroot\\").Last());
                        if (System.IO.File.Exists(fullPath))
                            System.IO.File.Delete(fullPath);
                    }

                    product.AdditionalImages.Remove(image);
                    dbContext.ProductImages.Remove(image);
                }
            }

            // Add new additional images
            if (additionalImages != null && additionalImages.Any())
            {
                foreach (var image in additionalImages)
                {
                    var imagePath = SaveImageAndGetPath(image, product.ProductId);
                    product.AdditionalImages.Add(new ProductImage { ImagePath = imagePath });
                }
            }

            await dbContext.SaveChangesAsync();
            var result = await MapProductAsync(product);
            return Ok(result);

        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await dbContext.Products
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

         
            var imageFolder = Path.Combine(hostingEnvironment.WebRootPath, "images", "products", id.ToString());
            if (Directory.Exists(imageFolder))
            {
                Directory.Delete(imageFolder, true);
            }

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}