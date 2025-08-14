using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController(IUserRoles userRoleRepo) : ControllerBase
    {
        private readonly IUserRoles _userRoleRepo = userRoleRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllUserRoles()
        {
            try
            {
                var result = await _userRoleRepo.GetAllUserRolesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserRoleById(int id)
        {
            try
            {
                var result = await _userRoleRepo.GetUserRolesByIdAsync(id);
                if (result == null)
                    return NotFound($"User Role with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserRole([FromBody] UserRoles userRoles)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _userRoleRepo.CreateUserRolesAsync(userRoles);
                return CreatedAtAction(nameof(GetUserRoleById), new { id = result?.UserRolesId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            try
            {
                var result = await _userRoleRepo.DeleteUserRoleAsync(id);
                if (result == null)
                    return NotFound($"User Role with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
