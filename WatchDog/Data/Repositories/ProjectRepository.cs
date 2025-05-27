using System;
using System.Collections.Generic;
using System.Linq;
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

        var query = $@"INSERT INTO Projects (Title, Description, CreatedDate, StartDate, EndDate, Status)
                       VALUES (@Title, @Description, @CreatedDate, @StartDate, @EndDate, @Status)
                       RETURNING Id;";

        return await connection.QuerySingleAsync<int>(query, new
        {
            project.Title,
            project.Description,
            project.CreatedDate,
            project.StartDate,
            project.EndDate,
            project.Status,
        });
    }

    public override async Task<bool> UpdateAsync(Project project)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                UPDATE Projects
                SET Status = @Status,
                    EndDate = CASE 
                                WHEN @Status = 'Completed' THEN CURRENT_TIMESTAMP 
                                ELSE EndDate 
                              END
                WHERE Id = @ProjectId";

            int rowsAffected = await connection.ExecuteAsync(
                query,
                new
                {
                    ProjectId = project.Id,
                    project.Status
                }
            );

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(UpdateAsync)}: {e.Message}");
        } 
    }
}