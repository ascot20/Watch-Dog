using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public class SessionService:ISessionService
{
    private User? _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public User GetCurrentUser()
    {
        return _currentUser;
    }

    public void SetCurrentUser(User user)
    {
        _currentUser = user;
    }

    public System.Threading.Tasks.Task ClearSessionAsync()
    {
        _currentUser = null;
        return System.Threading.Tasks.Task.CompletedTask;
    }
}