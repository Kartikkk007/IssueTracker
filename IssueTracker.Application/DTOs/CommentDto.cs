namespace IssueTracker.Application.DTOs;

public record CommentDto(
    int CommentId,
    int IssueId,
    int UserId,
    string UserName,
    string? UserAvatar,
    string Content,
    DateTime CreatedAt
);
