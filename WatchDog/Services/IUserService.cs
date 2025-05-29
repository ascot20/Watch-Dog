using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IUserService
{
    Task<int> RegisterAsync(string email, string username, string password = "default", UserRole role = UserRole.User);
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetUserAsync(int id);
    Task<string> GetUserNameAsync(int id);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> UserExistsAsync(int userId);
}