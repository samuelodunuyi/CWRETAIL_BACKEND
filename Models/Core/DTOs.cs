using System;

namespace CW_RETAIL.Models.Core
{
    public class StoreFullInfoDTO
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string StorePhoneNumber { get; set; }
        public string StoreEmailAddress { get; set; }
        public string StoreAddress { get; set; }
        public int? StoreAdminId { get; set; }
        public string StoreType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class StoreBasicInfoDTO
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreType { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserBasicInfoDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
    }
}