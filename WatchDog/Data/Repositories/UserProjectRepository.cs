using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class UserProjectRepository : IUserProjectRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UserProjectRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> AddAsync(int userId, int projectId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                INSERT INTO UserProjects (UserId, ProjectId)
                VALUES (@UserId, @ProjectId)
                ON CONFLICT (UserId, ProjectId) DO NOTHING;";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                ProjectId = projectId
            });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(AddAsync)}: {e.Message}");
        }
    }

    public async Task<bool> RemoveAsync(int userId, int projectId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                DELETE FROM UserProjects
                WHERE UserId = @UserId AND ProjectId = @ProjectId;";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                UserId = userId,
                ProjectId = projectId
            });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(RemoveAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                SELECT p.*
                FROM Projects p
                JOIN UserProjects up ON p.Id = up.ProjectId
                WHERE up.UserId = @UserId;";

            return await connection.QueryAsync<Project>(query, new { UserId = userId });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetProjectsByUserIdAsync)}: {e.Message}");
        }
    }

    public async Task<IEnumerable<User>> GetUsersByProjectIdAsync(int projectId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var query = @"
                SELECT u.*
                FROM Users u
                JOIN UserProjects up ON u.Id = up.UserId
                WHERE up.ProjectId = @ProjectId;";

            return await connection.QueryAsync<User>(query, new { ProjectId = projectId });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetUsersByProjectIdAsync)}: {e.Message}");
        }
    }
}