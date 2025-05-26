using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ITimeLineReplyRepository: IRepository<TimeLineReply>
{
    Task<IEnumerable<TimeLineReply>> GetByMessageIdAsync(int messageId);
    Task<TimeLineReply?> GetWithCreatorAsync(int replyId);
    Task<int> GetTotalCountForMessageAsync(int messageId);
}