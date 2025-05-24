using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ITimeLineMessageRepository: IRepository<TimeLineMessage>
{
    Task<IEnumerable<TimeLineMessage>> GetMessagesByProjectIdAsync(int projectId);
    Task<IEnumerable<TimeLineMessage>> GetMessagesByCreatorIdAsync(int creatorId);
    Task<IEnumerable<TimeLineMessage>> GetMessagesByTypeAsync(MessageType type);
    Task<IEnumerable<TimeLineMessage>> GetMessagesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<TimeLineMessage?> GetMessageWithCreatorAsync(int messageId);
    Task<int> GetTotalMessageCountForProjectAsync(int projectId);
}