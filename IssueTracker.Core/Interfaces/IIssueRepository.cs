namespace IssueTracker.Core.Interfaces;

public interface IIssueRepository<TIssue> where TIssue : class
{
    Task<List<TIssue>> GetIssuesByProjectAsync(int projectId, CancellationToken ct = default);
    Task<TIssue?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> GetNextIssueSequenceAsync(string projectKey, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int issueId, string newStatus, CancellationToken ct = default);
}