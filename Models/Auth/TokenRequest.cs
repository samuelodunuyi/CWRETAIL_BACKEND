using System.ComponentModel.DataAnnotations;

namespace CW_RETAIL.Models.Auth
{
    public class TokenRequest
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}