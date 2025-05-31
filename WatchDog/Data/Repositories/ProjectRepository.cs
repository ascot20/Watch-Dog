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
        try
        {
            await base.CreateAsync(project);
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = $@"INSERT INTO Projects (Title, Description, CreatedDate, StartDate, EndDate, Status)
                       VALUES (@Title, @Description, @CreatedDate, @StartDate, @EndDate, @Status::project_status)
                       RETURNING Id;";

            return await connection.QuerySingleAsync<int>(query, new
            {
                project.Title,
                project.Description,
                project.CreatedDate,
                project.StartDate,
                project.EndDate,
                Status = project.Status.ToString()
            });
        }
        catch (Exception e)
        {
            throw new Exception("Error creating project");
        }
    }

    public async Task<bool> ExistsByTitleAsync(string title)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                SELECT COUNT(1)
                FROM projects
                WHERE LOWER(title) = LOWER(@Title);";

            int count = await connection.ExecuteScalarAsync<int>(query, new { Title = title });

            return count > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override async Task<bool> UpdateAsync(Project project)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                UPDATE Projects
                SET Status = @Status::project_status,
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
                    Status = project.Status.ToString()
                }
            );

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw new Exception($"Could not update project");
        }
    }
}