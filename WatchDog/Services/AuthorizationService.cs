using System;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class AuthorizationService : IAuthorizationService
{
    private User? _currentUser;
    private readonly ITaskRepository _taskRepository;

    public AuthorizationService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public bool IsAdmin()
    {
        return _currentUser?.Role == UserRole.SuperAdmin;
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
            var task = await _taskRepository.GetByIdAsync(taskId);
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
        catch (Exception e)
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