using System;
using System.Collections.Generic;

namespace IssueTracker.Infrastructure.Entities;

public partial class Issue
{
    public int IssueId { get; set; }

    public int ProjectId { get; set; }

    public string IssueKey { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Priority { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int ReporterId { get; set; }

    public int? AssigneeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? Assignee { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User Reporter { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
