using System;
using System.Collections.Generic;
using System.Linq;
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
        if (progressionMessage == null)
        {
            throw new ArgumentNullException(nameof(progressionMessage), "ProgressionMessage cannot be null");
        }

        if (string.IsNullOrWhiteSpace(progressionMessage.Content))
        {
            throw new ArgumentException("Content cannot be empty", nameof(progressionMessage));
        }

        if (progressionMessage.SubTaskId <= 0)
        {
            throw new ArgumentException("SubTask ID must be a positive number", nameof(progressionMessage));
        }

        try
        {
            await base.CreateAsync(progressionMessage);

            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                INSERT INTO ProgressionMessages (Content, SubTaskId, AuthorId, CreatedDate)
                VALUES (@Content, @SubTaskId, @AuthorId, @CreatedDate)
                RETURNING Id";

            return await connection.QuerySingleAsync<int>(query, new
            {
                progressionMessage.Content,
                progressionMessage.SubTaskId,
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

    public async Task<IEnumerable<ProgressionMessage>> GetBySubTaskIdAsync(int subTaskId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<ProgressionMessage>(
                "SELECT * FROM ProgressionMessages WHERE SubTaskId = @SubTaskId ORDER BY CreatedDate DESC",
                new { SubTaskId = subTaskId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetBySubTaskIdAsync)}: {e.Message}");
        }
 
    }

    public async Task<int> GetTotalCountForSubTaskAsync(int subTaskId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ProgressionMessages WHERE SubTaskId = @SubTaskId",
                new { SubTaskId = subTaskId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetTotalCountForSubTaskAsync)}: {e.Message}");
        }
    }
}