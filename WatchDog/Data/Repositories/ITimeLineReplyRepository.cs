using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ITimeLineReplyRepository: IRepository<TimeLineReply>
{
    Task<IEnumerable<TimeLineReply>> GetByMessageIdAsync(int messageId);
    Task<IEnumerable<TimeLineReply>> GetByCreatorIdAsync(int creatorId);
    Task<int> GetTotalCountForMessageAsync(int messageId);
}