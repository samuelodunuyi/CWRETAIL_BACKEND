namespace CWSERVER.Models.Core.DTOs
{
    public class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
    }
}