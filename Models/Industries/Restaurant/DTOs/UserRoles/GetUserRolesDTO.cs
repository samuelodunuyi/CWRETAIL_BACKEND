using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.UserRoles
{
    public class GetUserRolesDTO
    {
        public int UserRolesId { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public string? Role { get; set; }

        [ForeignKey("Stores")]
        public int StoreId { get; set; }
        [ForeignKey("User")]
        public string? AssignedBy { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
