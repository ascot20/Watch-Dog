using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class ProgressionMessageRepository : Repository<ProgressionMessage>, IProgressionMessageRepository
{
    public ProgressionMessageRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "ProgressionMessages")
    {
    }

    public override async Task<int> CreateAsync(ProgressionMessage progressionMessage)
    {
        try
        {
            await base.CreateAsync(progressionMessage);

            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                INSERT INTO ProgressionMessages (Content, Taskid, AuthorId, CreatedDate)
                VALUES (@Content, @TaskId, @AuthorId, @CreatedDate)
                RETURNING Id";

            return await connection.QuerySingleAsync<int>(query, new
            {
                progressionMessage.Content,
                progressionMessage.TaskId,
                progressionMessage.AuthorId,
                progressionMessage.CreatedDate,
            });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<ProgressionMessage>> GetByCreatorIdAsync(int creatorId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<ProgressionMessage>(
                "SELECT * FROM ProgressionMessages WHERE AuthorId = @AuthorId ORDER BY CreatedDate DESC",
                new { AuthorId = creatorId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByCreatorIdAsync)}: {e.Message}");
        }
 
    }

    public async Task<IEnumerable<ProgressionMessage>> GetByTaskIdAsync(int taskId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<ProgressionMessage>(
                "SELECT * FROM ProgressionMessages WHERE TaskId = @TaskId ORDER BY CreatedDate DESC",
                new { TaskId = taskId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByTaskIdAsync)}: {e.Message}");
        }
 
    }

    public async Task<int> GetTotalCountForTaskAsync(int taskId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM progressionmessages WHERE TaskId = @SubTaskId",
                new { TaskId = taskId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetTotalCountForTaskAsync)}: {e.Message}");
        }
    }
}