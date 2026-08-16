using IssueTracker.Core.Interfaces;
using IssueTracker.Infrastructure.Context;
using IssueTracker.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Repositories;

public class IssueRepository : IIssueRepository<Issue>
{
    private readonly IssueTrackerDbContext _context;

    public IssueRepository(IssueTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<Issue>> GetIssuesByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId)
            .Include(i => i.Assignee)
            .Include(i => i.Tags)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Issue?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.IssueId == id, ct);
    }

    public async Task<int> GetNextIssueSequenceAsync(string projectKey, CancellationToken ct = default)
    {
        var count = await _context.Issues
            .CountAsync(i => i.IssueKey.StartsWith(projectKey + "-"), ct);
        return count + 1;
    }

    public async Task<bool> UpdateStatusAsync(int issueId, string newStatus, CancellationToken ct = default)
    {
        var issue = await _context.Issues.FindAsync(new object[] { issueId }, ct);
        if (issue is null) return false;

        issue.Status = newStatus;
        issue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}