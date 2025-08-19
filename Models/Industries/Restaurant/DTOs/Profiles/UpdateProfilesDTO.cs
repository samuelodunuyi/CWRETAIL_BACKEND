namespace CWSERVER.Models.Industries.Restaurant.DTOs.Profiles
{
    public class UpdateProfilesDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarURL { get; set; }
        public string? Email { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
