using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CWSERVER.Models.Industries.Restaurant.DTOs.AuditLogs
{
    public class GetAuditLogDTO
    {
        public int AuditLogId { get; set; }
        [Required]
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
