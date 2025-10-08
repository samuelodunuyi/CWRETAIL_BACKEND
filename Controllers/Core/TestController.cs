using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok(new { Message = "This endpoint is accessible without authentication" });
        }

        [HttpGet("auth")]
        [Authorize]
        public IActionResult GetAuth()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            
            return Ok(new { 
                Message = "You are authenticated!", 
                UserEmail = currentUserEmail,
                UserRole = currentUserRole
            });
        }
    }
}