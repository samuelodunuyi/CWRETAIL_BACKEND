namespace CWSERVER.Models.Industries.Restaurant.DTOs.Stores
{
    public class UpdateStoreDTO
    {
        public string? Name { get; set; }
        public string? Location { get; set; }

        //[ForeignKey("Mangager")]
        //public int ManagerId { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
