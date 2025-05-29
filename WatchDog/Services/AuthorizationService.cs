using System;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public class AuthorizationService : IAuthorizationService
{
    private User? _currentUser;
    private readonly ITaskService _taskService;

    public AuthorizationService(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public bool IsAdmin()
    {
        return _currentUser?.Role == UserRole.SuperAdmin;
    }

    public string GetCurrentUserName()
    {
        return _currentUser?.Username ?? "Anonymous";
    }
    
    public async Task<bool> IsUserAuthorizedForTask(int taskId)
    {
        if (IsAdmin())
        {
            return true;
        }

        if (_currentUser == null)
        {
            return false;
        }

        try
        {
            var task = await _taskService.GetTaskAsync(taskId);
            if (task == null)
            {
                return false;
            }

            if (task.AssignedUserId == _currentUser.Id)
            {
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public int GetCurrentUserId()
    {
        return _currentUser?.Id ?? 0;
    }

    public void SetCurrentUser(User user)
    {
        _currentUser = user;
    }
    
    public void ClearCurrentUser()
    {
        _currentUser = null;
    }
}