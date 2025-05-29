using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IAuthorizationService
{
    bool IsAdmin();
    Task<bool> IsUserAuthorizedForTask(int taskId);
    int GetCurrentUserId();
    string GetCurrentUserName();
    void SetCurrentUser(User user);
    void ClearCurrentUser();
}