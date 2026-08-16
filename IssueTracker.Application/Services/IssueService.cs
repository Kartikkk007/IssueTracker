using IssueTracker.Application.DTOs;
using IssueTracker.Infrastructure.Context;
using IssueTracker.Infrastructure.Entities;
using IssueTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Application.Services;

public class IssueService
{
    private readonly IssueRepository _issueRepository;
    private readonly IssueTrackerDbContext _context;

    public IssueService(IssueRepository issueRepository, IssueTrackerDbContext context)
    {
        _issueRepository = issueRepository;
        _context = context;
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
            i.Assignee?.FullName,
            i.CreatedAt,
            i.Tags.Select(t => t.Name).ToList()
        )).ToList();
    }

    public async Task<bool> ChangeStatusAsync(int issueId, string newStatus)
    {
        return await _issueRepository.UpdateStatusAsync(issueId, newStatus);
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _context.Users.AsNoTracking().ToListAsync();
    }

    public async Task<bool> CreateIssueAsync(CreateIssueCommand command)
    {
        var project = await _context.Projects.FindAsync(command.ProjectId);
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

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();
        return true;
    }
}