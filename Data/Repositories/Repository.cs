using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WatchDog.Data.Factories;
using WatchDog.Models;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.Data.Repositories;

public abstract class Repository<T> : IRepository<T> where T:BaseEntity
{
    protected readonly IDbConnectionFactory _dbConnectionFactory;
    protected readonly string _tableName;

    protected Repository(IDbConnectionFactory dbConnectionFactory, string tableName)
    {
        this._dbConnectionFactory = dbConnectionFactory;
        this._tableName = tableName;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(
            $"SELECT * FROM {_tableName} WHERE Id = @Id",
            new {Id = id}
        );
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(
            $"SELECT * FROM {_tableName}"
        );
    }

    public virtual async Task<int> CreateAsync(T entity)
    {
        if (entity is AuditableEntity auditableEntity)
        {
            auditableEntity.CreatedDate = DateTime.UtcNow;
        }
        
        return await Task.FromResult(0);
    }
    
    public virtual async Task DeleteAsync(int id)
    {
        using var connection = this._dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            $"DELETE FROM {_tableName} WHERE Id = @Id",
            new { Id = id }
        );
    }
    
}