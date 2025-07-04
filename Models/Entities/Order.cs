using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public virtual Store? Store { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public int Status { get; set; } = 0;
        public string? CreatedBy { get; set; } 

        public DateTime? LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedBy { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}