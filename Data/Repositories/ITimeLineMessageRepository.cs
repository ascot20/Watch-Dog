using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface ITimeLineMessageRepository: IRepository<TimeLineMessage>
{
    Task<IEnumerable<TimeLineMessage>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<TimeLineMessage>> GetByCreatorIdAsync(int creatorId);
    Task<IEnumerable<TimeLineMessage>> GetByTypeAsync(MessageType type);
    Task<IEnumerable<TimeLineMessage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<TimeLineMessage?> GetWithCreatorAsync(int messageId);
    Task<int> GetTotalCountForProjectAsync(int projectId);
}