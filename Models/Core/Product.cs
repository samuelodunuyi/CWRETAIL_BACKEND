using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CW_RETAIL.Models.Core
{
    public class Product
    {
        [Key]
        [JsonIgnore]
        public int ProductId { get; set; }
        
        [Required]
        public string ProductName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string? SKU { get; set; }
        
        public string? Barcode { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }

        [Column(TypeName="decimal(18, 2)")]
        public decimal BasePrice { get; set; }
        
        [Column(TypeName="decimal(18, 2)")]
        public decimal CostPrice { get; set; }
        
        public int CurrentStock { get; set; }
        
        public int MinimumStockLevel { get; set; }
        
        public int MaximumStockLevel { get; set; }
        
        public string? UnitOfMeasure { get; set; }
        
        public string? ImageUrl { get; set; }

        public virtual ICollection<ProductImage> AdditionalImages { get; set; } = new List<ProductImage>();

        public bool ShowInWeb { get; set; } = true;
        public bool ShowInPOS { get; set; } = true;
        public bool IsActive { get; set; } = true;
        
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }
        
        public string? CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Category? Category { get; set; }
        public virtual Store? Store { get; set; }
    }
}