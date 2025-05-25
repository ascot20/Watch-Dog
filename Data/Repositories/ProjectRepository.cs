using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "Projects")
    {
    }

    public override async Task<int> CreateAsync(Project project)
    {
        await base.CreateAsync(project);
        using var connection = _dbConnectionFactory.CreateConnection();

        var query = $@"INSERT INTO {this._tableName} (Title, Description, CreatedDate, StartDate, EndDate, Status)
                       VALUES (@Title, @Description, @CreatedDate, @StartDate, @EndDate, @Status);
                       RETURNING Id";

        return await connection.QuerySingleAsync<int>(query, new
        {
            project.Title,
            project.Description,
            project.CreatedDate,
            project.StartDate,
            project.EndDate,
            Status = (int)project.Status,
        });
    }

    public async Task<IEnumerable<Project>> GetByUserIdAsync(int userId)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        return await connection.QueryAsync<Project>($@"
            SELECT * FROM Projects
            JOIN UserProjects ON Users.Id = UserProjects.ProjectId
            WHERE UserProjects.UserId = @UserId",
            new { UserId = userId }
        );
    }

    public Task<Project?> GetWithMembersAsync(int projectId)
    {
        throw new System.NotImplementedException();
    }

    public Task<Project?> GetWithTasksAsync(int projectId)
    {
        throw new System.NotImplementedException();
    }
}