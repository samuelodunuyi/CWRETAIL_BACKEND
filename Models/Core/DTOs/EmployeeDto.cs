namespace CWSERVER.Models.Core.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public StoreDto? Store { get; set; }
    }

    public class StoreDto
    {
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? StoreRep { get; set; }
    }
}
