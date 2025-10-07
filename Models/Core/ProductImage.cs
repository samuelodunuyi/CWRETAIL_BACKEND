using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CW_RETAIL.Models.Core
{
    public class ProductImage
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        
        public string? ImagePath { get; set; }
        
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        
        [JsonIgnore]
        public virtual Product? Product { get; set; }
    }
}