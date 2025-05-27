using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ITimeLineMessageService
{
    Task<int> CreateMessageAsync(TimeLineMessage message);
    Task<TimeLineMessage?> GetMessageAsync(int messageId);
    Task<bool> UpdateMessageAsync(TimeLineMessage message);
    Task<bool> DeleteMessageAsync(int messageId);
}