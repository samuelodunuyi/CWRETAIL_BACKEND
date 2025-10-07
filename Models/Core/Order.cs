using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CW_RETAIL.Models.Core
{
    [Table("Orders")]
    public class Order
    {
        // Order Status Constants
        public const int STATUS_PENDING = 0;
        public const int STATUS_CONFIRMED = 1;
        public const int STATUS_COMPLETED = 2;
        public const int STATUS_AWAITING_DELIVERY = 3;
        public const int STATUS_DELIVERED = 4;
        public const int STATUS_FAILED = 5;
        public const int STATUS_RETURNED = 6;
        
        [Key]
        public int Id { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        
        public virtual Store? Store { get; set; }
        
        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        
        public virtual User? Customer { get; set; }
        
        public int Status { get; set; } = STATUS_PENDING;
        
        public int? PaymentOption { get; set; }
        
        public string? TransactionRef { get; set; }
        
        [Required]
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? LastUpdatedAt { get; set; } = DateTime.UtcNow;
        
        public string? LastUpdatedBy { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}