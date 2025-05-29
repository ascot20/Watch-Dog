using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class SubTaskRepository : Repository<SubTask>, ISubTaskRepository
{
    public SubTaskRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "SubTasks")
    {
    }

    public override async Task<int> CreateAsync(SubTask progressionMessage)
    {
        try
        {
            await base.CreateAsync(progressionMessage);

            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                INSERT INTO SubTasks (Description, StartDate, Status, TaskId, CreatedById)
                VALUES (@Description, @TaskId, @CreatedById)
                RETURNING Id;";

            return await connection.QuerySingleAsync<int>(query, new
            {
                progressionMessage.Description,
                progressionMessage.TaskId,
                progressionMessage.CreatedById,
            });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<SubTask>> GetByTaskIdAsync(int taskId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<SubTask>(
                "SELECT * FROM SubTasks WHERE TaskId = @TaskId ORDER BY CreatedDate",
                new { TaskId = taskId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByTaskIdAsync)}: {e.Message}");
        }
    }

    public async Task<int> GetCountForTaskAsync(int taskId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM SubTasks WHERE TaskId = @TaskId",
                new { TaskId = taskId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetCountForTaskAsync)}: {e.Message}");
        }
    }

    public override async Task<bool> UpdateAsync(SubTask subTask)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                UPDATE SubTasks
                SET Description = @Description,
                    Status = @Status,
                    CompletedDate = CASE 
                                        WHEN @Status='Completed' THEN CURRENT_TIMESTAMP
                                        ELSE completeddate
                                    END   
                WHERE Id = @Id";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                subTask.Description,
                subTask.Status,
                subTask.Id
            });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(UpdateAsync)}: {e.Message}");
        }
    }
}