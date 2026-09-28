using IssueTracker.Core.Interfaces;
using IssueTracker.Infrastructure.Context;
using IssueTracker.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Repositories;

public class IssueRepository : IIssueRepository<Issue>
{
    private readonly IDbContextFactory<IssueTrackerDbContext> _contextFactory;

    public IssueRepository(IDbContextFactory<IssueTrackerDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Issue>> GetIssuesByProjectAsync(int projectId, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Issues
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId)
            .Include(i => i.Assignee)
            .Include(i => i.Tags)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Issue?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.IssueId == id, ct);
    }

    public async Task<int> GetNextIssueSequenceAsync(string projectKey, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        var keys = await context.Issues
            .AsNoTracking()
            .Where(i => i.IssueKey.StartsWith(projectKey + "-"))
            .Select(i => i.IssueKey)
            .ToListAsync(ct);

        int maxSeq = 0;
        foreach (var key in keys)
        {
            var parts = key.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int seq))
            {
                if (seq > maxSeq)
                {
                    maxSeq = seq;
                }
            }
        }
        return maxSeq + 1;
    }

    public async Task<bool> UpdateStatusAsync(int issueId, string newStatus, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        var issue = await context.Issues.FindAsync(new object[] { issueId }, ct);
        if (issue is null) return false;

        issue.Status = newStatus;
        issue.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<Issue>> GetAllIssuesAsync(CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Issues
            .AsNoTracking()
            .Include(i => i.Assignee)
            .Include(i => i.Project)
            .Include(i => i.Tags)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateIssueAsync(Issue updatedIssue, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        var issue = await context.Issues.FindAsync(new object[] { updatedIssue.IssueId }, ct);
        if (issue is null) return false;

        issue.Title = updatedIssue.Title;
        issue.Description = updatedIssue.Description;
        issue.Priority = updatedIssue.Priority;
        issue.Status = updatedIssue.Status;
        issue.AssigneeId = updatedIssue.AssigneeId;
        issue.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteIssueAsync(int issueId, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        var issue = await context.Issues.FindAsync(new object[] { issueId }, ct);
        if (issue is null) return false;

        context.Issues.Remove(issue);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<Project>> GetProjectsAsync(CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Projects
            .AsNoTracking()
            .Include(p => p.Issues)
            .OrderBy(p => p.ProjectId)
            .ToListAsync(ct);
    }

    public async Task<bool> CreateProjectAsync(Project project, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        project.CreatedAt = DateTime.UtcNow;
        context.Projects.Add(project);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteProjectAsync(int projectId, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        var project = await context.Projects
            .Include(p => p.Issues)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, ct);
        if (project is null) return false;

        if (project.Issues != null && project.Issues.Any())
        {
            var issueIds = project.Issues.Select(i => i.IssueId).ToList();
            var comments = await context.Comments.Where(c => issueIds.Contains(c.IssueId)).ToListAsync(ct);
            if (comments.Any())
            {
                context.Comments.RemoveRange(comments);
            }
            context.Issues.RemoveRange(project.Issues);
        }

        context.Projects.Remove(project);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task LogActivityAsync(int? issueId, string? issueKey, string action, string description, string userName = "Alex Rivers", CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        var log = new ActivityLog
        {
            IssueId = issueId,
            IssueKey = issueKey,
            Action = action,
            Description = description,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        };

        context.ActivityLogs.Add(log);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<ActivityLog>> GetRecentActivitiesAsync(int limit = 10, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.ActivityLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<ActivityLog>> GetActivitiesForIssueAsync(int issueId, CancellationToken ct = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.IssueId == issueId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }
}