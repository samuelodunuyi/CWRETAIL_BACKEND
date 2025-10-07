using System.ComponentModel.DataAnnotations;

namespace CW_RETAIL.Models.Auth
{
    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}