using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ISubTaskRepository: IRepository<SubTask>
{
    Task<IEnumerable<SubTask>> GetSubTasksByTaskIdAsync(int taskId);
    Task<bool> UpdateSubTaskStatusAsync(int subTaskId, SubTaskStatus newStatus);
    Task<int> GetSubTaskCountForTaskAsync(int taskId);
}