using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public virtual Store? Store { get; set; }

        public string? MainImagePath { get; set; } 
        public virtual ICollection<ProductImage> AdditionalImages { get; set; } = new List<ProductImage>();
        public string? ProductLabel { get; set; }
        public int ProductAmountInStock { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductOriginalPrice { get; set; }
        public string? ProductDescription { get; set; }
        [Required]
        public string? ProductSKU { get; set; }
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}