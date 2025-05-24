using System.Collections.Generic;
using System.Threading.Tasks;
using Task = WatchDog.Models.Task;

namespace WatchDog.Data.Repositories;

public interface ITaskRepository: IRepository<Task>
{
    Task<IEnumerable<Task>> GetTasksByProjectIdAsync(int projectId);
    Task<IEnumerable<Task>> GetTasksByAssignedUserIdAsync(int userId);
    Task<Task?> GetTaskWithAssigneeAsync(int taskId);
    Task<bool> AssignTaskToUserAsync(int taskId, int userId);
    Task<bool> UnassignTaskAsync(int taskId);
}