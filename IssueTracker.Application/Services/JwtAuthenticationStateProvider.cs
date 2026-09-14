using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace IssueTracker.Application.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly JwtTokenService _jwtTokenService;
    private AuthenticationState _currentState;

    public JwtAuthenticationStateProvider(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
        _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_currentState);
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var principal = _jwtTokenService.ValidateToken(token);
        if (principal != null)
        {
            _currentState = new AuthenticationState(principal);
        }
        else
        {
            _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public void MarkUserAsLoggedOut()
    {
        _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }
}
