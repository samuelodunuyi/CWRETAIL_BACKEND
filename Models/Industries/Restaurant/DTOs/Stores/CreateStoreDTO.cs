namespace CWSERVER.Models.Industries.Restaurant.DTOs.Stores
{
    public class CreateStoreDTO
    {
        public string? Name { get; set; }
        public string? Location { get; set; }

        //[ForeignKey("Mangager")]
        //public int ManagerId { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
