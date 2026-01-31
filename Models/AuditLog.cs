using System;

namespace IT_13FinalProject.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
        public string? Details { get; set; }
    }
}
