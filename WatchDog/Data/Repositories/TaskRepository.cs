using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;
using Task = WatchDog.Models.Task;

namespace WatchDog.Data.Repositories;

public class TaskRepository : Repository<Models.Task>, ITaskRepository
{
    public TaskRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "Tasks")
    {
    }

    public async Task<IEnumerable<Models.Task>> GetByProjectIdAsync(int projectId)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            var query = @"
                SELECT t.*, u.Username AS AssignedUserName
                FROM tasks t 
                LEFT JOIN users u ON t.assigneduserid = u.id
                WHERE t.projectid = @ProjectId
                ORDER BY t.startdate DESC, t.id DESC";

            return await connection.QueryAsync<Models.Task>(
                query,
                new { ProjectId = projectId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByProjectIdAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<Models.Task>> GetByAssignedUserIdAsync(int userId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<Models.Task>(
                "SELECT * FROM Tasks WHERE AssignedUserId = @UserId ORDER BY StartDate",
                new { UserId = userId }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByAssignedUserIdAsync)}: {e.Message}");
        }
    }

    public override async Task<int> CreateAsync(Models.Task task)
    {
        try
        {
            await base.CreateAsync(task);
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                    INSERT INTO Tasks (TaskDescription, ProjectId, AssignedUserId)
                    VALUES (@TaskDescription, @ProjectId, @AssignedUserId)
                    RETURNING Id;";

            return await connection.QuerySingleAsync<int>(query, new
            {
                task.TaskDescription,
                task.ProjectId,
                task.AssignedUserId,
            });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message}");
        }
    }

    public override async Task<bool> UpdateAsync(Models.Task task)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                    UPDATE Tasks 
                    SET Remarks = @Remarks,
                        PercentageComplete = @PercentageComplete,
                        CompletedDate = CASE 
                            WHEN @PercentageComplete = 100 THEN CURRENT_TIMESTAMP 
                                ELSE CompletedDate 
                                    END ,
                        AssignedUserId = @AssignedUserId
                    WHERE Id = @Id";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                task.Remarks,
                task.PercentageComplete,
                task.AssignedUserId,
                task.Id
            });
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Database error in {nameof(UpdateAsync)}: {e.Message}");
            throw new Exception($"Database error in {nameof(UpdateAsync)}: {e.Message}");
        }
    }
}