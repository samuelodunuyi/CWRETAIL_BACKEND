using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.DTOs
{
    public class RegisterEmployeeRequest : RegisterRequest
    {
        [Required]
        public int StoreId { get; set; }
    }
}