using CW_RETAIL.Models.Auth;
using CW_RETAIL.Models.Core;
using CW_RETAIL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var result = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (result.user == null)
            {
                if (result.errorMessage.Contains("inactive"))
                {
                    return StatusCode(403, new { Message = result.errorMessage });
                }
                return Unauthorized(new { Message = result.errorMessage });
            }

            return Ok(new
            {
                AccessToken = result.accessToken,
                RefreshToken = result.refreshToken,
                Username = result.user.Username,
                Email = result.user.Email,
                Role = result.user.RoleId,
                FirstName = result.user.FirstName,
                LastName = result.user.LastName

            });
        }

        // Default register endpoint removed as requested

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest model)
        {
            var result = await _authService.RefreshTokenAsync(model.AccessToken, model.RefreshToken);

            if (result.accessToken == null || result.refreshToken == null)
            {
                return BadRequest(new { Message = "Invalid token" });
            }

            return Ok(new
            {
                AccessToken = result.accessToken,
                RefreshToken = result.refreshToken
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity.Name;
            var result = await _authService.RevokeRefreshTokenAsync(username);

            if (!result)
            {
                return BadRequest(new { Message = "Invalid user" });
            }

            return Ok(new { Message = "Logged out successfully" });
        }
        
        // Customer registration endpoint (open, no authorization required)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            // Set role to Customer
            model.RoleId = 3; // Customer role ID

            var result = await _authService.RegisterAsync(
                model.Username, 
                model.Email, 
                model.Password, 
                model.RoleId,
                model.FirstName,
                model.LastName);

            if (!result)
            {
                return BadRequest(new { Message = "User already exists" });
            }

            return Ok(new { Message = "Customer registered successfully" });
        }
        
        // Change password endpoint (requires authentication)
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            var username = User.Identity.Name;
            
            // Verify old password and change to new password
            var result = await _authService.ChangePasswordAsync(username, model.OldPassword, model.NewPassword);
            
            if (!result)
            {
                return BadRequest(new { Message = "Invalid old password" });
            }
            
            return Ok(new { Message = "Password changed successfully" });
        }
    }
}
