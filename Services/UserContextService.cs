using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CWSERVER.Services
{
    public interface IUserContextService
    {
        string GetCurrentUserId();
        string GetCurrentUsername();
    }

    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId ?? "System";
        }

        public string GetCurrentUsername()
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            return username ?? "System";
        }
    }
}