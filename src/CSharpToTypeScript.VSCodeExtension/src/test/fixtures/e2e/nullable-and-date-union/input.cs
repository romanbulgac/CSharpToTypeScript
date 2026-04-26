using System;

namespace Contracts
{
    public class AuditDto
    {
        public DateTime? ProcessedAt { get; set; }
        public DateTimeOffset? SyncedAt { get; set; }
        public int? RetryCount { get; set; }
        public string? Error { get; set; }
    }
}
