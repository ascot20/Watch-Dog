using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ITimeLineMessageService
{
    Task<int> CreateMessageAsync(
        string message,
        int creatorId,
        int projectId,
        MessageType type = MessageType.Question,
        bool isPinned = false);
    Task<TimeLineMessage?> GetMessageAsync(int messageId);
    Task<IEnumerable<TimeLineMessage>> GetMessagesByProjectIdAsync(int projectId);
    Task<bool> UpdateMessageAsync(int messageId, MessageType type, bool isPinned);
    Task<bool> DeleteMessageAsync(int messageId);
    Task<bool> MessageExistsAsync(int messageId);
}