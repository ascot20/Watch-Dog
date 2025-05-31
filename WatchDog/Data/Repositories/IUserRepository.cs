using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> SearchAsync(string searchTerm);
}