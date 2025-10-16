using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.NutriionalInfo;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class NutritionInfoController(INutritionalInfo nutritionalInfoRepo) : ControllerBase
    {
        private readonly INutritionalInfo _nutritionalInfo = nutritionalInfoRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllNutritionalInfo()
        {
            try
            {
                var result = await _nutritionalInfo.GetAllNutritionalInfoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNutritionalInfoById(int id)
        {
            try
            {
                var result = await _nutritionalInfo.GetNutritionalInfoByIdAsync(id);
                if (result == null)
                    return NotFound($"Nutrition info with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNutritionalInfo([FromBody] NutritionalInfo nutritionalInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _nutritionalInfo.CreateNutritionalInfoAsync(nutritionalInfo);
                return CreatedAtAction(nameof(GetNutritionalInfoById), new { id = result?.NutritionalInfoId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateNutritionalInfo(int id, [FromBody] UpdateNutritionalInfo updateNutritionalInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _nutritionalInfo.UpdateNutritionalInfoAsync(id, updateNutritionalInfo);
                if (result == null)
                    return NotFound($"Nutrition info with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNutritionalInfo(int id)
        {
            try
            {
                var result = await _nutritionalInfo.DeleteNutritionalInfoAsync(id);
                if (result == null)
                    return NotFound($"Nutrition info with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
