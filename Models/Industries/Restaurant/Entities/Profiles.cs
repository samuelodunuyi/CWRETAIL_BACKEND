using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class Profiles
    {
        [Key]
        public int ProfileId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarURL { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
