using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Core.DTOs
{
    public class RegisterEmployeeRequest : RegisterRequest
    {
        [Required]
        public int StoreId { get; set; }
    }
}