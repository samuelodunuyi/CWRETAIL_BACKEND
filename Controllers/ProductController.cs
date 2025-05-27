using CWSERVER.Data;
using CWSERVER.Models.Entities;
using CWSERVER.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace CWSERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApiDbContext dbContext;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ProductController(
            ApiDbContext dbContext,
            IWebHostEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.hostingEnvironment = hostingEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        private string SaveImageAndGetPath(IFormFile imageFile, int productId)
        {
            var uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images", "products", productId.ToString());
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

            var request = httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var relativePath = Path.Combine("images", "products", productId.ToString(), uniqueFileName).Replace("\\", "/");

            return $"{baseUrl}/{relativePath}";
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllProducts(
            [FromQuery] int? categoryId,
            [FromQuery] int? storeId,
            [FromQuery] decimal? productPriceFrom,
            [FromQuery] decimal? productPriceTo,
            [FromQuery] string? productSKU)
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
                AdditionalImageUrls = p.AdditionalImages.Select(i => i.ImagePath).ToList(),
                ProductLabel = p.ProductLabel,
                ProductAmountInStock = p.ProductAmountInStock,
                ProductPrice = p.ProductPrice,
                ProductOriginalPrice = p.ProductOriginalPrice,
                ProductDescription = p.ProductDescription,
                ProductSKU = p.ProductSKU
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
                AdditionalImageUrls = product.AdditionalImages.Select(i => i.ImagePath).ToList(),
                ProductLabel = product.ProductLabel,
                ProductAmountInStock = product.ProductAmountInStock,
                ProductPrice = product.ProductPrice,
                ProductOriginalPrice = product.ProductOriginalPrice,
                ProductDescription = product.ProductDescription,
                ProductSKU = product.ProductSKU
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDTO productDto,
            [FromForm] IFormFile? mainImage,
            [FromForm] List<IFormFile>? additionalImages)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                ProductName = productDto.ProductName,
                CategoryId = productDto.CategoryId,
                StoreId = productDto.StoreId,
                ProductLabel = productDto.ProductLabel,
                ProductAmountInStock = productDto.ProductAmountInStock,
                ProductPrice = productDto.ProductPrice,
                ProductOriginalPrice = productDto.ProductOriginalPrice,
                ProductDescription = productDto.ProductDescription,
                ProductSKU = productDto.ProductSKU
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            
            if (mainImage != null)
            {
                var imagePath = SaveImageAndGetPath(mainImage, product.ProductId);
                product.MainImagePath = imagePath;
            }

            
            if (additionalImages != null && additionalImages.Any())
            {
                foreach (var image in additionalImages)
                {
                    var imagePath = SaveImageAndGetPath(image, product.ProductId);
                    product.AdditionalImages.Add(new ProductImage { ImagePath = imagePath });
                }
            }

            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductCreateDTO productDto,
            [FromForm] IFormFile? mainImage,
            [FromForm] List<IFormFile>? additionalImages)
        {
            var product = await dbContext.Products
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            product.ProductName = productDto.ProductName;
            product.CategoryId = productDto.CategoryId;
            product.StoreId = productDto.StoreId;
            product.ProductLabel = productDto.ProductLabel;
            product.ProductAmountInStock = productDto.ProductAmountInStock;
            product.ProductPrice = productDto.ProductPrice;
            product.ProductOriginalPrice = productDto.ProductOriginalPrice;
            product.ProductDescription = productDto.ProductDescription;
            product.ProductSKU = productDto.ProductSKU;

           
            if (mainImage != null)
            {
                var imagePath = SaveImageAndGetPath(mainImage, product.ProductId);
                product.MainImagePath = imagePath;
            }

         
            if (additionalImages != null && additionalImages.Any())
            {
               

                foreach (var image in additionalImages)
                {
                    var imagePath = SaveImageAndGetPath(image, product.ProductId);
                    product.AdditionalImages.Add(new ProductImage { ImagePath = imagePath });
                }
            }

            await dbContext.SaveChangesAsync();

            return NoContent();
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