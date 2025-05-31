using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CWSERVER.Data;
using CWSERVER.Models.DTOs;
using CWSERVER.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CWSERVER.Services
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken, string accessTokenFromHeader);
        Task LogoutAsync(string userId);
        Task<bool> IsTokenRevoked(string jwtId);
    }

    public class AuthService(
        ApiDbContext context,
        UserManager<User> userManager,
        IConfiguration config) : IAuthService
    {
        private readonly ApiDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IConfiguration _config = config;

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password!))
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User account is inactive");

            return await GenerateTokens(user);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string accessTokenFromHeader)
        {
            var principal = GetPrincipalFromExpiredToken(accessTokenFromHeader);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Invalid user");

            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

            if (storedRefreshToken == null || storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked)
                throw new UnauthorizedAccessException("Invalid refresh token");

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                throw new UnauthorizedAccessException("Refresh token expired");

          
            storedRefreshToken.IsUsed = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            return await GenerateTokens(user);
        }

        public async Task LogoutAsync(string userId)
        {
  
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsUsed && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
            }

            if (refreshTokens.Any())
            {
                _context.RefreshTokens.UpdateRange(refreshTokens);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsTokenRevoked(string jwtId)
        {
            return await _context.RefreshTokens
                .AnyAsync(rt => rt.JwtId == jwtId && (rt.IsUsed || rt.IsRevoked));
        }

        private async Task<TokenResponse> GenerateTokens(User user)
        {
            var jwtId = Guid.NewGuid().ToString();
            var accessToken = GenerateAccessToken(user, jwtId);
            var refreshToken = GenerateRefreshToken();

       
            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                JwtId = jwtId,
                IsUsed = false,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(1) 
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessToken.ValidTo
            };
        }

        private JwtSecurityToken GenerateAccessToken(User user, string jwtId)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Role, user.Role),
                new(JwtRegisteredClaimNames.Jti, jwtId),
                new(JwtRegisteredClaimNames.Sub, user.Email!)
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _config["JwtConfig:Issuer"],
                audience: _config["JwtConfig:Audience"],
                expires: DateTime.UtcNow.AddMinutes(15), 
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]!)),
                ValidateLifetime = false,
                ValidIssuer = _config["JwtConfig:Issuer"],
                ValidAudience = _config["JwtConfig:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}