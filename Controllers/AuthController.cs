using CWSERVER.Models.DTOs;
using CWSERVER.Models.Entities;
using CWSERVER.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CWSERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, UserManager<User> userManager) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly UserManager<User> _userManager = userManager;

        [HttpPost("token")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var tokens = await _authService.LoginAsync(request);
                return Ok(tokens);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
                var accessToken = authHeader?.Split(' ').Last(); 

                var tokens = await _authService.RefreshTokenAsync(request.RefreshToken!, accessToken!);
                return Ok(tokens);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _authService.LogoutAsync(userId!);
            return NoContent();
        }
    }
}