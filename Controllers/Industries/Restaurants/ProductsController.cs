using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProducts productRepo) : ControllerBase
    {
        private readonly IProducts _productRepo = productRepo;

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
        public async Task<IActionResult> CreateProduct([FromBody] Models.Industries.Restaurant.Entities.Products product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
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
    }
}
