using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.Data.Repositories;

public interface IRepository<T> where T:BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}