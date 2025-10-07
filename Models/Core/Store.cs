using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CW_RETAIL.Models.Core
{
    public class Store
    {
        [Key]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int StoreId { get; set; }
        
        [Required]
        public string? StoreName { get; set; }

        public string? StorePhoneNumber { get; set; }

        public string? StoreEmailAddress { get; set; }

        public string? StoreAddress { get; set; }

        public string? StoreAdmin { get; set; }
        
        public int? UserId { get; set; }
        
        [Required]
        public string StoreType { get; set; } = "Supermarket";
        
        public bool IsActive { get; set; } = true;
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime? UpdatedAt { get; set; }
    }
}