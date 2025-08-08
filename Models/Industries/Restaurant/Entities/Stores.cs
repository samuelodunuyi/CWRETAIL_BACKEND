using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class Stores
    {
        public int StoresId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }

        //[ForeignKey("Mangager")]
        //public int ManagerId { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt {  get; set; }
    }
}
