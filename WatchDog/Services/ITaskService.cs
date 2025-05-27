using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WatchDog.Services;

public interface ITaskService
{
    Task<int> CreateTaskAsync(Models.Task task);
    Task<Models.Task?> GetTaskAsync(int taskId);
    Task<bool> UpdateTaskAsync(Models.Task task);
    Task<bool> DeleteTaskAsync(int taskId);
}