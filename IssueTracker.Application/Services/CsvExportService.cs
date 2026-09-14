using System.Text;
using IssueTracker.Infrastructure.Entities;

namespace IssueTracker.Application.Services;

public class CsvExportService
{
    public byte[] ExportIssuesToCsv(IEnumerable<Issue> issues)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Issue Key,Title,Status,Priority,Project,Assignee,Created At");

        foreach (var issue in issues)
        {
            var key = EscapeCsv(issue.IssueKey);
            var title = EscapeCsv(issue.Title);
            var status = EscapeCsv(issue.Status.ToString());
            var priority = EscapeCsv(issue.Priority.ToString());
            var project = EscapeCsv(issue.Project?.Name ?? "");
            var assignee = EscapeCsv(issue.Assignee?.FullName ?? "Unassigned");
            var createdAt = EscapeCsv(issue.CreatedAt.ToString("yyyy-MM-dd HH:mm"));

            sb.AppendLine($"{key},{title},{status},{priority},{project},{assignee},{createdAt}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportProjectsToCsv(IEnumerable<Project> projects)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Project Name,Project Key,Description,Created At");

        foreach (var project in projects)
        {
            var name = EscapeCsv(project.Name);
            var key = EscapeCsv(project.KeyPrefix);
            var desc = EscapeCsv(project.Description ?? "");
            var createdAt = EscapeCsv(project.CreatedAt.ToString("yyyy-MM-dd HH:mm"));

            sb.AppendLine($"{name},{key},{desc},{createdAt}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
