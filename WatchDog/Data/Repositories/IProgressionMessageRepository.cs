using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProgressionMessageRepository: IRepository<ProgressionMessage>
{
    Task<IEnumerable<ProgressionMessage>> GetByTaskIdAsync(int taskId);
    Task<IEnumerable<ProgressionMessage>> GetByCreatorIdAsync(int creatorId);
    Task<int> GetTotalCountForTaskAsync(int taskId);
}