using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class TimeLineMessageRepository: Repository<TimeLineMessage>, ITimeLineMessageRepository
{
    public TimeLineMessageRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "TimeLineMessages")
    {
        
    }

    public Task<IEnumerable<TimeLineMessage>> GetByCreatorIdAsync(int creatorId)
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<TimeLineMessage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TimeLineMessage>> GetByProjectIdAsync(int projectId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TimeLineMessage>> GetByTypeAsync(MessageType type)
    {
        throw new NotImplementedException();
    }

    public Task<TimeLineMessage?> GetWithCreatorAsync(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalCountForProjectAsync(int projectId)
    {
        throw new NotImplementedException();
    }
}