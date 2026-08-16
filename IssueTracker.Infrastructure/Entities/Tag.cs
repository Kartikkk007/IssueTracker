using System;
using System.Collections.Generic;

namespace IssueTracker.Infrastructure.Entities;

public partial class Tag
{
    public int TagId { get; set; }

    public string Name { get; set; } = null!;

    public string ColorHex { get; set; } = null!;

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
