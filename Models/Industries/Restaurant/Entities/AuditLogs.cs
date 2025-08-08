using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.Entities
{
    public class AuditLogs
    {
        public int AuditLogId { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
