using System;
using System.ComponentModel.DataAnnotations;

namespace CW_RETAIL.Models.Core
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }
        
        // Storing username as string instead of foreign key
        public string? Userid { get; set; }
        
        public string? Action { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}