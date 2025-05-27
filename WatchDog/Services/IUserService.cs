using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IUserService
{
    Task<int> RegisterAsync(User user, string password);
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetUserAsync(int id);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<IEnumerable<User>> GetAllAsync();
}