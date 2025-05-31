using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskService _taskService;
    private readonly IUserProjectRepository _userProjectRepository;
    private readonly IAuthorizationService _authorizationService;

    public UserService(IUserRepository userRepository, ITaskService taskService,
        IUserProjectRepository userProjectRepository, IAuthorizationService authorizationService)
    {
        this._userRepository = userRepository;
        this._taskService = taskService;
        this._userProjectRepository = userProjectRepository;
        this._authorizationService = authorizationService;
    }

    public async Task<int> RegisterAsync(
        string email,
        string username,
        string password = "default",
        UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only admins can register new users");
        }

        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new Exception("User already exists");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = UserRole.User
            };

            return await _userRepository.CreateAsync(newUser);
        }
        catch (Exception e)
        {
            throw new Exception($"Error registering user: {e.Message}");
        }
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            return null;
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        return isPasswordValid ? user : null;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user != null)
            {
                user.AssignedTasks = (await _taskService.GetByAssignedUserIdAsync(id)).ToList();
                user.UserProjects = (await _userProjectRepository.GetProjectsByUserIdAsync(id)).ToList();
            }

            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await this.GetAllAsync();
        }
        
        return await _userRepository.SearchAsync(searchTerm);
    }

    public async Task<string> GetUserNameAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} does not exist");
            }

            return user.Username;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving username for user ID {id}: {e.Message}", e);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting all users: {e.Message}");
        }
    }


    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            throw new ArgumentException("Old password cannot be empty", nameof(oldPassword));
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("New password cannot be empty", nameof(newPassword));
        }

        if (oldPassword == newPassword)
        {
            throw new ArgumentException("New password must be different from the old password", nameof(newPassword));
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return false;
            }

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordHash = newPasswordHash;

            return await _userRepository.UpdateAsync(user);
        }
        catch (Exception e)
        {
            throw new Exception($"Error changing password: {e.Message}");
        }
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }
        catch (Exception e)
        {
            throw new Exception($"Error checking if user exists: {e.Message}");
        }
    }
}