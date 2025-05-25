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

    public override async Task<int> CreateAsync(User user)
    {
        await base.CreateAsync(user);
        
        using var connection = this._dbConnectionFactory.CreateConnection();

        var query = @"
            INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedDate)
            VALUES (@Username, @Email, @PasswordHash, @Role, @CreatedDate);
            RETURNING Id";

        return await connection.QuerySingleAsync<int>(query, new
        {
            user.Username,
            user.Email,
            user.PasswordHash,
            Role = (int) user.Role
        });
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email",
            new { Email = email }
        );
    }

    public async Task<User?> GetWithAssignedTasksAsync(int userId)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();

        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users  WHERE Id = @UserId",
            new { UserId = userId }
        );

        if (user == null)
        {
            return null;
        }

        var tasks = await connection.QueryAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE AssignedUserId = @UserId",
            new { UserId = userId }
            );

        user.AssignedTasks = tasks.ToList();

        return user;
    }
}