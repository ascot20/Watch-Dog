using System.Collections.Generic;
using System.Threading.Tasks;

namespace WatchDog.Services;

public interface ITaskService
{
    Task<int> CreateTaskAsync(string description, int projectId, int assignedUserId);
    Task<Models.Task?> GetTaskAsync(int taskId);

    Task<bool> UpdateTaskAsync(
        int taskId,
        string? remarks = null,
        int? percentageCompleted = null,
        int? assignedUserId = null);

    Task<bool> DeleteTaskAsync(int taskId);
    Task<IEnumerable<Models.Task>> GetByAssignedUserIdAsync(int userId);
    Task<IEnumerable<Models.Task>> GetByProjectIdAsync(int projectId);
    Task<bool> TaskExistsAsync(int taskId);
}