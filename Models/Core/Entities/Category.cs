using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Core.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string? CategoryIcon { get; set; }
        [Required]
        public string? CategoryName { get; set; }
        
        // Make StoreId nullable for general categories
        [ForeignKey("Store")]
        public int? StoreId { get; set; }
        public virtual Store? Store { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
