using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IUserService
{
    Task<int> RegisterAsync(User user, string password);
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithAssignedTasksAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<IEnumerable<User>> GetAllAsync();
}