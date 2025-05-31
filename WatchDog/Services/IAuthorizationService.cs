namespace WatchDog.Services;

public interface IAuthorizationService
{
    bool IsAdmin();
    int GetCurrentUserId();
    string GetCurrentUserName();
}