using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Profiles;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class ProfilesController(IProfiles profileRepo) : ControllerBase
    {
        private readonly IProfiles _profileRepo = profileRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllProfiles()
        {
            try
            {
                var result = await _profileRepo.GetAllProfileAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileById(int id)
        {
            try
            {
                var result = await _profileRepo.GetProfilesByIdAsync(id);
                if (result == null)
                    return NotFound($"Profile with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] Profiles profiles)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _profileRepo.CreateProfileAsync(profiles);
                return CreatedAtAction(nameof(GetProfileById), new { id = result?.ProfileId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfilesDTO updateProfilesDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _profileRepo.UpdateProfileAsync(id, updateProfilesDTO);
                if (result == null)
                    return NotFound($"Profile with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            try
            {
                var result = await _profileRepo.DeleteProfileAsync(id);
                if (result == null)
                    return NotFound($"Profile with ID {id} not found.");
                return Ok($"Profile with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
