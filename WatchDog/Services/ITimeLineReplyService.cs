using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ITimeLineReplyService
{
    Task<int> CreateReplyAsync(string reply, int creatorId, int messageId);
    Task<IEnumerable<TimeLineReply>> GetByMessageIdAsync(int messageId);
}