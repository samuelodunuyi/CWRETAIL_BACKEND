
namespace CWSERVER.Models.DTOs
{
    public class UserDataResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }
        public EmployeeDto? EmployeeData { get; set; }
        public CustomerDto? CustomerData { get; set; }
    }
}