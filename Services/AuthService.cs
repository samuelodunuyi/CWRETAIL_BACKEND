using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CW_RETAIL.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<(User user, string accessToken, string refreshToken, string errorMessage)> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return (null, null, null, "Invalid username or password");
            }
            
            // Check if user is active
            if (!user.IsActive)
            {
                return (null, null, null, "Account is inactive. Please contact an administrator.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return (user, accessToken, refreshToken, null);
        }

        public async Task<bool> RegisterAsync(string username, string email, string password, int roleId)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                return false;
            }

            CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                RoleId = roleId,
                RefreshToken = _jwtService.GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name;

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return (null, null);
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();

            return (newAccessToken, newRefreshToken);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();

            return true;
        }
        
        public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            
            if (user == null || !VerifyPasswordHash(oldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return false;
            }
            
            CreatePasswordHash(newPassword, out string passwordHash, out string passwordSalt);
            
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            
            await _context.SaveChangesAsync();
            
            return true;
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(saltBytes);
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return computedHash == storedHash;
        }
    }
}