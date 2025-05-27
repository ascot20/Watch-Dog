using System.Collections.Generic;
using System.Threading.Tasks;
using Task = WatchDog.Models.Task;

namespace WatchDog.Data.Repositories;

public interface ITaskRepository: IRepository<Task>
{
    Task<IEnumerable<Models.Task>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<Models.Task>> GetByAssignedUserIdAsync(int userId);
}