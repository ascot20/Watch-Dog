using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class TimeLineMessageRepository: Repository<TimeLineMessage>, ITimeLineMessageRepository
{
    public TimeLineMessageRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "TimeLineMessages")
    {
        
    }

    public override async Task<int> CreateAsync(TimeLineMessage timeLineMessage)
    {
        if (timeLineMessage == null)
        {
            throw new ArgumentNullException(nameof(timeLineMessage), "TimeLineMessage cannot be null");
        }

        if (string.IsNullOrWhiteSpace(timeLineMessage.Content))
        {
            throw new ArgumentException("Content cannot be empty", nameof(timeLineMessage));
        }

        try
        {
            await base.CreateAsync(timeLineMessage);

            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                INSERT INTO TimeLineMessages (Content, Type, IsPinned, ProjectId, AuthorId, CreatedDate)
                VALUES (@Content, @Type, @IsPinned, @ProjectId, @AuthorId, @CreatedDate)
                RETURNING Id";

            return await connection.QuerySingleAsync<int>(query, new
            {
                timeLineMessage.Content,
                timeLineMessage.Type,
                timeLineMessage.IsPinned,
                timeLineMessage.ProjectId,
                timeLineMessage.AuthorId,
                timeLineMessage.CreatedDate
            });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message}");
        }
 
    }

    public async Task<IEnumerable<TimeLineMessage>> GetByCreatorIdAsync(int creatorId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<TimeLineMessage>(
                "SELECT * FROM TimeLineMessages WHERE AuthorId = @AuthorId ORDER BY CreatedDate DESC",
                new { AuthorId = creatorId }
            );
 
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByCreatorIdAsync)}: {e.Message}");
        }
 
    }

    public async Task<IEnumerable<TimeLineMessage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be before or equal to end date", nameof(startDate));
        }

        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<TimeLineMessage>(
                "SELECT * FROM TimeLineMessages WHERE CreatedDate BETWEEN @StartDate AND @EndDate ORDER BY CreatedDate DESC",
                new { StartDate = startDate, EndDate = endDate }
            );
 
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByDateRangeAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<TimeLineMessage>> GetByProjectIdAsync(int projectId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<TimeLineMessage>(
                "SELECT * FROM TimeLineMessages WHERE ProjectId = @ProjectId ORDER BY IsPinned DESC, Id DESC",
                new { ProjectId = projectId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByProjectIdAsync)}: {e.Message}");
        }
 
    }

    public async Task<IEnumerable<TimeLineMessage>> GetByTypeAsync(MessageType type)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<TimeLineMessage>(
                "SELECT * FROM TimeLineMessages WHERE Type = @Type ORDER BY Id DESC",
                new { Type = type }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByTypeAsync)}: {e.Message}");
        }

    }

    public async Task<int> GetTotalCountForProjectAsync(int projectId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM TimeLineMessages WHERE ProjectId = @ProjectId",
                new { ProjectId = projectId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetTotalCountForProjectAsync)}: {e.Message}");
        }
 
    }
    
    public override async Task<bool> UpdateAsync(TimeLineMessage timeLineMessage)
    {
        if (timeLineMessage == null)
        {
            throw new ArgumentNullException(nameof(timeLineMessage), "TimeLineMessage cannot be null");
        }

        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                UPDATE TimeLineMessages
                SET Type = @Type,
                    IsPinned = @IsPinned
                WHERE Id = @Id";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                timeLineMessage.Type,
                timeLineMessage.IsPinned,
                timeLineMessage.Id
            });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(UpdateAsync)}: {e.Message}");
        }
    }
 
}