using System;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly ISessionService _sessionService;

    public AuthorizationService( ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public bool IsAdmin()
    {
        var currentUser = _sessionService.GetCurrentUser();
        return currentUser?.Role == UserRole.SuperAdmin;
    }

    public string GetCurrentUserName()
    {
        var currentUser = _sessionService.GetCurrentUser();
        return currentUser?.Username ?? throw new UnauthorizedAccessException("No user is currently authenticated");
    }
    
    
    public int GetCurrentUserId()
    {
        var currentUser = _sessionService.GetCurrentUser();
        return currentUser?.Id ?? throw new UnauthorizedAccessException("No user is currently authenticated");
    }
}