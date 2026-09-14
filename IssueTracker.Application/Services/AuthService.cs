using IssueTracker.Infrastructure.Entities;

namespace IssueTracker.Application.Services;

public class AuthService
{
    private readonly IssueService _issueService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public User? CurrentUser { get; private set; }
    public string? CurrentToken { get; private set; }
    public bool IsAuthenticated => CurrentUser != null && !string.IsNullOrEmpty(CurrentToken);

    public event Action? OnChange;

    public AuthService(
        IssueService issueService,
        JwtTokenService jwtTokenService,
        JwtAuthenticationStateProvider authStateProvider)
    {
        _issueService = issueService;
        _jwtTokenService = jwtTokenService;
        _authStateProvider = authStateProvider;
    }

    public async Task InitializeAsync()
    {
        if (CurrentUser == null)
        {
            var users = await _issueService.GetUsersAsync();
            if (users.Any())
            {
                var defaultUser = users.FirstOrDefault(u => u.Role == "Admin") ?? users.First();
                SetUserAndGenerateJwt(defaultUser);
            }
        }
    }

    public async Task<(bool Success, string Message)> LoginWithCredentialsAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Please provide both email and password.");
        }

        var users = await _issueService.GetUsersAsync();
        var user = users.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            return (false, "User not found with this email.");
        }

        // Validate standard demo credentials or accept standard password
        bool passwordValid = password switch
        {
            "Admin@123" when user.Role == "Admin" => true,
            "Dev@123" when user.Role == "Developer" => true,
            "Qa@123" when user.Role == "QA Tester" => true,
            "Password@123" => true,
            _ => password.Length >= 4 // Flexible for testing
        };

        if (!passwordValid)
        {
            return (false, "Invalid password for this account.");
        }

        SetUserAndGenerateJwt(user);
        return (true, "Login successful.");
    }

    public async Task<bool> LoginAsRoleAsync(string role)
    {
        var users = await _issueService.GetUsersAsync();
        var user = users.FirstOrDefault(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            SetUserAndGenerateJwt(user);
            return true;
        }
        return false;
    }

    public async Task<bool> LoginAsync(int userId)
    {
        var users = await _issueService.GetUsersAsync();
        var user = users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            SetUserAndGenerateJwt(user);
            return true;
        }
        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentToken = null;
        _authStateProvider.MarkUserAsLoggedOut();
        NotifyStateChanged();
    }

    public bool HasRole(params string[] roles)
    {
        if (CurrentUser == null || roles == null || roles.Length == 0)
            return false;

        return roles.Any(r => CurrentUser.Role.Equals(r, StringComparison.OrdinalIgnoreCase));
    }

    private void SetUserAndGenerateJwt(User user)
    {
        CurrentUser = user;
        CurrentToken = _jwtTokenService.GenerateToken(user);
        _authStateProvider.MarkUserAsAuthenticated(CurrentToken);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
