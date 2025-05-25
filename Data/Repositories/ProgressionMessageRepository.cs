using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class ProgressionMessageRepository : Repository<ProgressionMessage>, IProgressionMessageRepository
{
    public ProgressionMessageRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "ProgressionMessages")
    {
    }

    public override Task<int> CreateAsync(ProgressionMessage progressionMessage)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ProgressionMessage>> GetByCreatorIdAsync(int creatorId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ProgressionMessage>> GetBySubTaskIdAsync(int subTaskId)
    {
        throw new NotImplementedException();
    }

    public Task<ProgressionMessage?> GetWithCreatorAsync(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<ProgressionMessage?> GetWithSubTaskAsync(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalCountForSubTaskAsync(int subTaskId)
    {
        throw new NotImplementedException();
    }
}