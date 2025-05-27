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
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<T>(
                $"SELECT * FROM {_tableName} WHERE Id = @Id",
                new {Id = id}
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetByIdAsync)}: {e.Message} ");
        }
        
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<T>(
                $"SELECT * FROM {_tableName}"
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(GetAllAsync)}: {e.Message} ");
        }
        
    }

    public virtual async Task<int> CreateAsync(T entity)
    {
        if (entity is AuditableEntity auditableEntity)
        {
            auditableEntity.CreatedDate = DateTime.UtcNow;
        }
        
        return await Task.FromResult(0);
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
        }

        if (entity.Id <= 0)
        {
            throw new ArgumentException("Entity ID must be a positive number", nameof(entity));
        }
        
        return await Task.FromResult(false);
    }

    public virtual async Task DeleteAsync(int id)
    {
        try
        {
            using var connection = this._dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                $"DELETE FROM {_tableName} WHERE Id = @Id",
                new { Id = id }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Database error in {nameof(DeleteAsync)}: {e.Message} ");
        }
        
    }
    
}