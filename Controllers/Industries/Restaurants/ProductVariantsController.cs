using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.ProductVariants;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantsController(IProductVariants productVariantsRepo) : ControllerBase
    {
        private readonly IProductVariants _productVariantsRepo = productVariantsRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllProductVariants()
        {
            try
            {
                var result = await _productVariantsRepo.GetAllProductVariantsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductVariantById(int id)
        {
            try
            {
                var result = await _productVariantsRepo.GetProductVariantsByIdAsync(id);
                if (result == null)
                    return NotFound($"Product variant with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductVariant([FromBody] ProductVariants productVariants)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _productVariantsRepo.CreateProductVariantsAsync(productVariants);
                return CreatedAtAction(nameof(GetProductVariantById), new { id = result?.ProductVariantId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProductVariant(int id, [FromBody] UpdateProductVariantsDTO updateProductVariantsDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _productVariantsRepo.UpdateProductVariantsAsync(id, updateProductVariantsDTO);
                if (result == null)
                    return NotFound($"Product variant with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductVariant(int id)
        {
            try
            {
                var result = await _productVariantsRepo.DeleteProductVariantsAsync(id);
                if (result == null)
                    return NotFound($"Product variant with ID {id} not found.");
                return Ok($"Product variant with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
