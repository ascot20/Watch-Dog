using System;
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
        try
        {
            await base.CreateAsync(user);
        
            using var connection = this._dbConnectionFactory.CreateConnection();

            var query = @"
            INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedDate)
            VALUES (@Username, @Email, @PasswordHash, @Role, @CreatedDate)
            RETURNING Id;";

            return await connection.QuerySingleAsync<int>(query, new
            {
                user.Username,
                user.Email,
                user.PasswordHash,
                user.Role
            });
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(CreateAsync)}: {e.Message} ");
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
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByEmailAsync)}: {e.Message} ");
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