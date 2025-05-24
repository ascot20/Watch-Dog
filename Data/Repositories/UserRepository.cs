using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class UserRepository: Repository<User>, IUserRepository
{
    public UserRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "Users")
    {
        
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            $"SELECT * FROM {this._tableName} WHERE Email = @Email",
            new { Email = email }
        );
    }

    public async Task<User?> GetUserWithAssignedTasksAsync(int userId, string taskTable)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();

        var user = await connection.QueryFirstOrDefaultAsync<User>(
            $"SELECT * FROM {this._tableName}  WHERE Id = @UserId",
            new { UserId = userId }
        );

        if (user == null)
        {
            return null;
        }

        var tasks = await connection.QueryAsync<Models.Task>(
            $"SELECT * FROM {taskTable} WHERE AssignedUserId = @UserId",
            new { UserId = userId }
            );

        user.AssignedTasks = tasks.ToList();

        return user;
    }

    public async Task<IEnumerable<User>> GetUsersByProjectIdAsync(int projectId, string userProjectTable)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        
        return await connection.QueryAsync<User>(
            @$"SELECT * 
               FROM {this._tableName} 
               JOIN {userProjectTable} ON {this._tableName}.Id = {userProjectTable}.UserId
               wHERE {userProjectTable}.ProjectId = @ProjectId",
            new { ProjectId = projectId }
        );
    }

    public async System.Threading.Tasks.Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();

        var query = @$"UPDATE {this._tableName} 
                       SET PasswordHash = @PasswordHash 
                       WHERE Id = @Id";

        await connection.ExecuteAsync(query, new
        {
            Id = userId,
            PasswordHash = newPasswordHash
        });
    }

    public override async Task<int> CreateAsync(User user)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();

        var query = $@"
            INSERT INTO {this._tableName}
            VALUES (@Username, @Email, @PasswordHash, @Role);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

        return await connection.QuerySingleAsync<int>(query, new
        {
            user.Username,
            user.Email,
            user.PasswordHash,
            user.Role
        });
    }
    
}