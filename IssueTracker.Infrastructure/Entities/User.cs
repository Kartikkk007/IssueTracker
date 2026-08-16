using System;
using System.Collections.Generic;

namespace IssueTracker.Infrastructure.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Issue> IssueAssignees { get; set; } = new List<Issue>();

    public virtual ICollection<Issue> IssueReporters { get; set; } = new List<Issue>();
}
