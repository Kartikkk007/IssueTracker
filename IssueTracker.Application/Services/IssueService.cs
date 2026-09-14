using IssueTracker.Application.DTOs;
using IssueTracker.Infrastructure.Context;
using IssueTracker.Infrastructure.Entities;
using IssueTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Services;

public class IssueService
{
    private readonly IssueRepository _issueRepository;
    private readonly IDbContextFactory<IssueTrackerDbContext> _contextFactory;

    public IssueService(IssueRepository issueRepository, IDbContextFactory<IssueTrackerDbContext> contextFactory)
    {
        _issueRepository = issueRepository;
        _contextFactory = contextFactory;
    }

    public async Task<List<IssueDto>> GetKanbanBoardAsync(int projectId)
    {
        var issues = await _issueRepository.GetIssuesByProjectAsync(projectId);

        return issues.Select(i => new IssueDto(
            i.IssueId,
            i.IssueKey,
            i.Title,
            i.Description,
            i.Priority,
            i.Status,
            i.AssigneeId,
            i.Assignee?.FullName,
            i.ProjectId,
            i.Project?.Name,
            i.CreatedAt,
            i.Tags.Select(t => t.Name).ToList()
        )).ToList();
    }

    public async Task<List<IssueDto>> GetAllIssuesAsync()
    {
        var issues = await _issueRepository.GetAllIssuesAsync();

        return issues.Select(i => new IssueDto(
            i.IssueId,
            i.IssueKey,
            i.Title,
            i.Description,
            i.Priority,
            i.Status,
            i.AssigneeId,
            i.Assignee?.FullName,
            i.ProjectId,
            i.Project?.Name,
            i.CreatedAt,
            i.Tags.Select(t => t.Name).ToList()
        )).ToList();
    }

    public async Task<bool> ChangeStatusAsync(int issueId, string newStatus)
    {
        var existing = await _issueRepository.GetByIdAsync(issueId);
        string oldStatus = existing?.Status ?? "Unknown";

        bool success = await _issueRepository.UpdateStatusAsync(issueId, newStatus);
        if (success && existing != null)
        {
            await _issueRepository.LogActivityAsync(issueId, existing.IssueKey, "Status Change", $"moved {existing.IssueKey} from {oldStatus} to {newStatus}");
        }
        return success;
    }

    public async Task<bool> UpdateIssueAsync(IssueDto dto)
    {
        var issue = new Issue
        {
            IssueId = dto.IssueId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            AssigneeId = dto.AssigneeId
        };

        bool success = await _issueRepository.UpdateIssueAsync(issue);
        if (success)
        {
            await _issueRepository.LogActivityAsync(dto.IssueId, dto.IssueKey, "Issue Updated", $"updated details for {dto.IssueKey}");
        }
        return success;
    }

    public async Task<bool> DeleteIssueAsync(int issueId)
    {
        var existing = await _issueRepository.GetByIdAsync(issueId);
        string key = existing?.IssueKey ?? $"Issue #{issueId}";

        bool success = await _issueRepository.DeleteIssueAsync(issueId);
        if (success)
        {
            await _issueRepository.LogActivityAsync(null, key, "Issue Deleted", $"deleted ticket {key}");
        }
        return success;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var users = await context.Users.ToListAsync();
        
        // Ensure the 3 standard demo users exist with the expected emails and roles
        var expectedUsers = new List<(string Name, string Email, string Role)>
        {
            ("Alex Rivera", "alex.admin@linear.dev", "Admin"),
            ("Sarah Chen", "sarah.dev@linear.dev", "Developer"),
            ("Liam Miller", "liam.qa@linear.dev", "QA Tester")
        };

        bool changed = false;

        foreach (var (name, email, role) in expectedUsers)
        {
            var existing = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                // Also check by name in case email differs in existing DB
                var byName = users.FirstOrDefault(u => u.FullName.Contains(name.Split(' ')[0], StringComparison.OrdinalIgnoreCase));
                if (byName != null)
                {
                    byName.Email = email;
                    byName.Role = role;
                    changed = true;
                }
                else
                {
                    var newUser = new User
                    {
                        FullName = name,
                        Email = email,
                        Role = role,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Users.Add(newUser);
                    users.Add(newUser);
                    changed = true;
                }
            }
            else if (existing.Role != role)
            {
                existing.Role = role;
                changed = true;
            }
        }

        if (changed)
        {
            await context.SaveChangesAsync();
            users = await context.Users.ToListAsync();
        }

        return users;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        return await _issueRepository.GetProjectsAsync();
    }

    public async Task<bool> CreateProjectAsync(string name, string keyPrefix, string? description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(keyPrefix)) return false;

        var project = new Project
        {
            Name = name.Trim(),
            KeyPrefix = keyPrefix.Trim().ToUpper(),
            Description = description?.Trim()
        };

        bool success = await _issueRepository.CreateProjectAsync(project);
        if (success)
        {
            await _issueRepository.LogActivityAsync(null, project.KeyPrefix, "Project Created", $"created project '{project.Name}' ({project.KeyPrefix})");
        }
        return success;
    }

    public async Task<bool> CreateIssueAsync(CreateIssueCommand command)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects.FindAsync(command.ProjectId);
        if (project == null) return false;

        int nextSeq = await _issueRepository.GetNextIssueSequenceAsync(project.KeyPrefix);
        string generatedKey = $"{project.KeyPrefix}-{nextSeq}";

        var issue = new Issue
        {
            ProjectId = command.ProjectId,
            IssueKey = generatedKey,
            Title = command.Title,
            Description = command.Description,
            Priority = command.Priority,
            Status = "Backlog",
            ReporterId = command.ReporterId,
            AssigneeId = command.AssigneeId,
            CreatedAt = DateTime.UtcNow
        };

        context.Issues.Add(issue);
        await context.SaveChangesAsync();

        await _issueRepository.LogActivityAsync(issue.IssueId, issue.IssueKey, "Issue Created", $"created ticket {issue.IssueKey}: '{issue.Title}'");
        return true;
    }

    public async Task<List<ActivityLog>> GetRecentActivitiesAsync(int limit = 10)
    {
        return await _issueRepository.GetRecentActivitiesAsync(limit);
    }

    public async Task<List<ActivityLog>> GetActivitiesForIssueAsync(int issueId)
    {
        return await _issueRepository.GetActivitiesForIssueAsync(issueId);
    }

    public async Task<List<CommentDto>> GetCommentsByIssueIdAsync(int issueId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Comments
            .Include(c => c.User)
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                c.CommentId,
                c.IssueId,
                c.UserId,
                c.User != null ? c.User.FullName : "Unknown User",
                c.User != null ? c.User.AvatarUrl : null,
                c.Content,
                c.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<bool> AddCommentAsync(int issueId, int userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return false;

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var comment = new Comment
            {
                IssueId = issueId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var issue = await context.Issues.FindAsync(issueId);
            var user = await context.Users.FindAsync(userId);
            if (issue != null)
            {
                var userName = user?.FullName ?? "A user";
                await _issueRepository.LogActivityAsync(issueId, issue.IssueKey, "Comment Added", $"{userName} commented: \"{(content.Length > 40 ? content[..40] + "..." : content)}\"");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] AddCommentAsync failed: {ex.Message}");
            return false;
        }
    }
}