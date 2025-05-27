using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ITimeLineReplyService
{
    Task<int> CreateReplyAsync(TimeLineReply reply);
}