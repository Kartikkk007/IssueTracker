using System;
using System.Collections.Generic;

namespace IssueTracker.Infrastructure.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string KeyPrefix { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
