using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ISubTaskRepository: IRepository<SubTask>
{
    Task<IEnumerable<SubTask>> GetByTaskIdAsync(int taskId);
    Task<int> GetCountForTaskAsync(int taskId);
}