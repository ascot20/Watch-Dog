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
            return await connection.QueryAsync<Models.Task>(
                "SELECT * FROM Tasks WHERE ProjectId = @ProjectId ORDER BY StartDate",
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
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        if (string.IsNullOrWhiteSpace(task.TaskDescription))
        {
            throw new ArgumentException("Task description cannot be empty", nameof(task));
        }

        try
        {
            await base.CreateAsync(task);
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                    INSERT INTO Tasks (TaskDescription, Remarks, StartDate, CompletedDate, 
                                     PercentageComplete, ProjectId, AssignedUserId, CreatedDate)
                    VALUES (@TaskDescription, @Remarks, @StartDate, @CompletedDate, 
                           @PercentageComplete, @ProjectId, @AssignedUserId, @CreatedDate)
                    RETURNING Id;";
            
            return await connection.QuerySingleAsync<int>(query, new
            {
                task.TaskDescription,
                task.Remarks,
                task.StartDate,
                task.CompletedDate,
                task.PercentageComplete,
                task.ProjectId,
                task.AssignedUserId,
                task.CreatedDate
            });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message}");
        }
    }

    public override async Task<bool> UpdateAsync(Models.Task task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        if (string.IsNullOrWhiteSpace(task.TaskDescription))
        {
            throw new ArgumentException("Task description cannot be empty", nameof(task));
        }

        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                    UPDATE Tasks 
                    SET TaskDescription = @TaskDescription, 
                        Remarks = @Remarks,
                        StartDate = @StartDate,
                        CompletedDate = @CompletedDate,
                        PercentageComplete = @PercentageComplete
                    WHERE Id = @Id";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                task.TaskDescription,
                task.Remarks,
                task.StartDate,
                task.CompletedDate,
                task.PercentageComplete,
                task.Id
            });
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(UpdateAsync)}: {e.Message}");
        }
    }
}