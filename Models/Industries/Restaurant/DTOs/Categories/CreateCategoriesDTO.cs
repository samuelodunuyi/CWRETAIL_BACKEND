using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.Categories
{
    public class CreateCategoriesDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        //[ForeignKey("Parent")]
        //public string? ParenttId { get; set; }

        [Required]
        public int? StoreId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
