using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ISessionService
{
    User GetCurrentUser();
    void SetCurrentUser(User user);
    System.Threading.Tasks.Task ClearSessionAsync();
    bool IsAuthenticated { get; }
}