using System;

namespace IssueTracker.Infrastructure.Entities;

public partial class ActivityLog
{
    public int ActivityLogId { get; set; }
    public int? IssueId { get; set; }
    public string? IssueKey { get; set; }
    public string Action { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string UserName { get; set; } = "Alex Rivers";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
