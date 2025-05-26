using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class TimeLineReplyRepository : Repository<TimeLineReply>, ITimeLineReplyRepository
{
    public TimeLineReplyRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "TimeLineReplies")
    {
    }

    public override Task<int> CreateAsync(TimeLineReply entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TimeLineReply>> GetByMessageIdAsync(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<TimeLineReply?> GetWithCreatorAsync(int replyId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalCountForMessageAsync(int messageId)
    {
        throw new NotImplementedException();
    }
}