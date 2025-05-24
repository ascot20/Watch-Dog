using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProgressionMessageRepository: IRepository<ProgressionMessage>
{
    Task<IEnumerable<ProgressionMessage>> GetMessagesBySubTaskIdAsync(int subTaskId);
    Task<IEnumerable<ProgressionMessage>> GetMessagesByCreatorIdAsync(int creatorId);
    Task<ProgressionMessage?> GetMessageWithCreatorAsync(int messageId);
    Task<ProgressionMessage?> GetMessageWithSubTaskAsync(int messageId);
    Task<int> GetTotalMessageCountForSubTaskAsync(int subTaskId);
}