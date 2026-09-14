using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Infrastructure.Entities;

public class Comment
{
    [Key]
    public int CommentId { get; set; }

    public int IssueId { get; set; }

    public int UserId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IssueId))]
    public virtual Issue? Issue { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
