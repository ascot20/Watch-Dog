using System.Threading.Tasks;

namespace WatchDog.Services;

public interface IAuthorizationService
{
    bool IsAdmin();
    int GetCurrentUserId();
    string GetCurrentUserName();
    Task LogoutAsync();
}