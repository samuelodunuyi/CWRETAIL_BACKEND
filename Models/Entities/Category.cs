using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string? CategoryIcon { get; set; }
        [Required]
        public string? CategoryName { get; set; }
    }
}
