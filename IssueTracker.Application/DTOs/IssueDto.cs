namespace IssueTracker.Application.DTOs;

public record IssueDto(
    int IssueId,
    string IssueKey,
    string Title,
    string? Description,
    string Priority,
    string Status,
    string? AssigneeName,
    DateTime CreatedAt,
    List<string> Tags
);

public class CreateIssueCommand
{
    public int ProjectId { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public int ReporterId { get; set; } = 1;
    public int? AssigneeId { get; set; }
}