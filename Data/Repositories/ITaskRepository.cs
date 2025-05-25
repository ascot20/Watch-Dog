using System.Collections.Generic;
using System.Threading.Tasks;
using Task = WatchDog.Models.Task;

namespace WatchDog.Data.Repositories;

public interface ITaskRepository: IRepository<Task>
{
    Task<IEnumerable<Models.Task>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<Models.Task>> GetByAssignedUserIdAsync(int userId);
    Task<Task?> GetWithAssigneeAsync(int taskId);
    Task UpdateAssigneeAsync(int taskId, int? assigneeId);
    Task UpdatePercentageCompleteAsync(int taskId, int percentageComplete);
}