using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, "Users")
    {
    }

    public override async Task<int> CreateAsync(User user)
    {
        try
        {
            await base.CreateAsync(user);

            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
            INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedDate)
            VALUES (@Username, @Email, @PasswordHash, @Role::user_role, @CreatedDate)
            RETURNING Id;";

            return await connection.QuerySingleAsync<int>(query, new
            {
                user.Username,
                user.Email,
                user.PasswordHash,
                Role = user.Role.ToString(),
                user.CreatedDate
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Database error in {nameof(CreateAsync)}: {e.Message} ");
            throw new Exception($"Database error occurred in creating new user");
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = email }
            );
        }
        catch (Npgsql.NpgsqlException e)
        {
            throw new Exception("Error connecting to database");
        }
        catch (Exception e)
        {
            throw new Exception($"User does not exist");
        }
    }

    public async Task<IEnumerable<User>> SearchAsync(string searchTerm)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();

            var normalizedSearchTerm = $"%{searchTerm.ToLower()}%";

            var query = @"
                SELECT * FROM Users
                WHERE LOWER(Username) LIKE @SearchTerm
                OR LOWER(Email) LIKE @SearchTerm
                ORDER BY Username";

            return await connection.QueryAsync<User>(query, new { SearchTerm = normalizedSearchTerm });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(SearchAsync)}: {e.Message} ");
        }
    }

    public override async Task<bool> UpdateAsync(User user)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
                UPDATE Users
                SET PasswordHash = @PasswordHash
                WHERE Id = @Id";

            int rowsAffected = await connection.ExecuteAsync(query, new
            {
                user.PasswordHash,
                user.Id
            });

            return rowsAffected > 0;
        }

        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(UpdateAsync)}: {e.Message} ");
        }
    }
}