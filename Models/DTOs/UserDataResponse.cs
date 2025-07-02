
namespace CWSERVER.Models.DTOs
{
    public class UserDataResponse
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }

        //added these
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        //

        public EmployeeDto? EmployeeData { get; set; }
        public CustomerDto? CustomerData { get; set; }
    }
}