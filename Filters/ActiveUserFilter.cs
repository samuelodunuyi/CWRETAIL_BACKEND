using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using CWSERVER.Models.Entities;

namespace CWSERVER.Filters
{
    public class ActiveUserFilter : IAsyncActionFilter
    {
        private readonly UserManager<User> _userManager;

        public ActiveUserFilter(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    context.Result = new UnauthorizedObjectResult("User account is inactive");
                    return;
                }
            }

            await next();
        }
    }
}