using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController(IPermissions permissionsRepo) : ControllerBase
    {
        private readonly IPermissions _permissionsRepo = permissionsRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var result = await _permissionsRepo.GetAllPermissionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            try
            {
                var result = await _permissionsRepo.GetPermissionsByIdAsync(id);
                if (result == null)
                    return NotFound($"Permission with ID {id} not found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] Permissions permission)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _permissionsRepo.CreatePermissionsAsync(permission);
                return CreatedAtAction(nameof(GetPermissionById), new { id = result?.PermissionId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                var result = await _permissionsRepo.DeletePermissionAsync(id);
                if (result == null)
                    return NotFound($"Permission with ID {id} not found.");
                return Ok($"Permission with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
