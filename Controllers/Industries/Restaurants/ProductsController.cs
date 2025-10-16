using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Products;
using CWSERVER.Models.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CWSERVER.Data;
using Microsoft.EntityFrameworkCore;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class ProductsController(IProducts productRepo, ApiDbContext dbContext) : ControllerBase
    {
        private readonly IProducts _productRepo = productRepo;
        private readonly ApiDbContext _dbContext = dbContext;

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var result = await _productRepo.GetAllProductsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var result = await _productRepo.GetProductsByIdAsync(id);
                if (result == null)
                    return NotFound($"Product with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductsDTO createProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                // Validate StoreId exists
                if (createProductDto.StoreId != null)
                {
                    var storeExists = await _dbContext.Storess.AnyAsync(s => s.StoresId == createProductDto.StoreId);
                    if (!storeExists)
                    {
                        return BadRequest($"Store with ID {createProductDto.StoreId} does not exist.");
                    }
                }

                // Map DTO to entity
                var product = new Products
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    SKU = createProductDto.SKU,
                    Barcode = createProductDto.Barcode,
                    CategoryId = createProductDto.CategoryId,
                    StoreId = createProductDto.StoreId,
                    BasePrice = createProductDto.BasePrice,
                    CostPrice = createProductDto.CostPrice,
                    MinimumStockLevel = createProductDto.MinimumStockLevel,
                    MaximumStockLevel = createProductDto.MaximumStockLevel,
                    UnitOfMeasure = createProductDto.UnitOfMeasure,
                    IsRecipe = createProductDto.IsRecipe,
                    PrepTimeMinutes = createProductDto.PrepTimeMinutes,
                    CookingTimeMinutes = createProductDto.CookingTimeMinutes,
                    Allergens = createProductDto.Allergens,
                    SpiceLevel = createProductDto.SpiceLevel,
                    IsVegetarian = createProductDto.IsVegetarian,
                    IsVegan = createProductDto.IsVegan,
                    IsGlutenFree = createProductDto.IsGlutenFree,
                    Status = createProductDto.Status,
                    ImageUrl = createProductDto.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Ensure new products start with 0 stock - use restocking endpoint to add inventory
                product.CurrentStock = 0;
                
                var result = await _productRepo.CreateProductsAsync(product);
                return CreatedAtAction(nameof(GetProductById), new { id = result?.ProductId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductsDTO updateProductsDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _productRepo.UpdateProductsAsync(id, updateProductsDTO);
                if (result == null)
                    return NotFound($"Product with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productRepo.DeleteProductsAsync(id);
                if (result == null)
                    return NotFound($"Product with ID {id} not found.");
                return Ok($"Product with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{id}/restock")]
        public async Task<IActionResult> RestockProduct(int id, [FromBody] ProductRestockDTO restockDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _productRepo.RestockProductAsync(id, restockDto);
                if (result == null)
                    return NotFound($"Product with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
