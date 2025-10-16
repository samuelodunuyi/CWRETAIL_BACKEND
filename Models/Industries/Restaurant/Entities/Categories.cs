using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        //[ForeignKey("Parent")]
        //public string? ParenttId { get; set; }

        [ForeignKey("Stores")]
        public int? StoreId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
