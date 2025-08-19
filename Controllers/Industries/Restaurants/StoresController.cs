using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Stores;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController(IStores storeRepo) : ControllerBase
    {
        private readonly IStores _storeRepo = storeRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllStores()
        {
            try
            {
                var result = await _storeRepo.GetAllStoresAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStoreById(int id)
        {
            try
            {
                var result = await _storeRepo.GetStoresByIdAsync(id);
                if (result == null)
                    return NotFound($"Store with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] Stores store)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _storeRepo.CreateStoresAsync(store);
                return CreatedAtAction(nameof(GetStoreById), new { id = result?.StoresId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreDTO updateStoreDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _storeRepo.UpdateStoresAsync(id, updateStoreDTO);
                if (result == null)
                    return NotFound($"Store with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            try
            {
                var result = await _storeRepo.DeleteStoresByIdAsync(id);
                if (result == null)
                    return NotFound($"Store with ID {id} not found.");
                return Ok($"Store with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
