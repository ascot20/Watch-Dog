using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ITimeLineReplyRepository: IRepository<TimeLineReply>
{
    Task<IEnumerable<TimeLineReply>> GetRepliesByMessageIdAsync(int messageId);
    Task<TimeLineReply?> GetReplyWithCreatorAsync(int replyId);
    Task<int> GetTotalReplyCountForMessageAsync(int messageId);
}