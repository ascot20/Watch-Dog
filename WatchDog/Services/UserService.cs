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
        string password,
        UserRole role)
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
            Role = role
        };

        return await _userRepository.CreateAsync(newUser);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty");
        }

        var user = _userRepository.GetByEmailAsync(email);
        return user;
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
        var user = await _userRepository.GetByIdAsync(id);

        if (user != null)
        {
            user.AssignedTasks = (await _taskService.GetByAssignedUserIdAsync(id)).ToList();
            user.UserProjects = (await _userProjectRepository.GetProjectsByUserIdAsync(id)).ToList();
        }

        return user;
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
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {id} does not exist");
        }

        return user.Username;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users;
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

    public async Task<bool> UserExistsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null;
    }
}