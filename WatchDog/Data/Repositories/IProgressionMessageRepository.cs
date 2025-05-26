using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProgressionMessageRepository: IRepository<ProgressionMessage>
{
    Task<IEnumerable<ProgressionMessage>> GetBySubTaskIdAsync(int subTaskId);
    Task<IEnumerable<ProgressionMessage>> GetByCreatorIdAsync(int creatorId);
    Task<ProgressionMessage?> GetWithCreatorAsync(int messageId);
    Task<ProgressionMessage?> GetWithSubTaskAsync(int messageId);
    Task<int> GetTotalCountForSubTaskAsync(int subTaskId);
}