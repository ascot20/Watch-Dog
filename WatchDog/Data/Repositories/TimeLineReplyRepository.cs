using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class TimeLineReplyRepository : Repository<TimeLineReply>, ITimeLineReplyRepository
{
    public TimeLineReplyRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "TimeLineReplies")
    {
    }

    public override async Task<int> CreateAsync(TimeLineReply timeLineReply)
    {
        try
        {
            await base.CreateAsync(timeLineReply);

            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                INSERT INTO TimeLineReplies (Content, TimeLineMessageId, AuthorId, CreatedDate)
                VALUES (@Content, @TimeLineMessageId, @AuthorId, @CreatedDate)
                RETURNING Id";

            return await connection.QuerySingleAsync<int>(query, new
            {
                timeLineReply.Content,
                timeLineReply.TimeLineMessageId,
                timeLineReply.AuthorId,
                timeLineReply.CreatedDate
            });
 
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message}");
        }
 
    }

    public async Task<IEnumerable<TimeLineReply>> GetByMessageIdAsync(int messageId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<TimeLineReply>(
                "SELECT * FROM TimeLineReplies WHERE TimeLineMessageId = @MessageId ORDER BY CreatedDate",
                new { MessageId = messageId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByMessageIdAsync)}: {e.Message}");
        }
 
    }

    public async Task<int> GetTotalCountForMessageAsync(int messageId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM TimeLineReplies WHERE TimeLineMessageId = @MessageId",
                new { MessageId = messageId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetTotalCountForMessageAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<TimeLineReply>> GetByCreatorIdAsync(int creatorId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
        
            var timeLineReplies = await connection.QueryAsync<TimeLineReply>(
                "SELECT * FROM TimeLineReplies WHERE Authorid = @CreatorId ORDER BY CreatedDate DESC",
                new { CreatorId = creatorId }
            );
        
            return timeLineReplies;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByCreatorIdAsync)}: {e.Message}");
        }
 
    }
}