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
    private readonly ITaskRepository _taskRepository;
    private readonly IUserProjectRepository _userProjectRepository;

    public UserService(IUserRepository userRepository, ITaskRepository taskRepository,
        IUserProjectRepository userProjectRepository)
    {
        this._userRepository = userRepository;
        this._taskRepository = taskRepository;
        this._userProjectRepository = userProjectRepository;
    }

    public async Task<int> RegisterAsync(User user, string password = "default")
    {
        try
        {
            var existingUser = _userRepository.GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new Exception("User already exists");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            return await _userRepository.CreateAsync(user);
        }
        catch (Exception e)
        {
            throw new Exception($"Error registering user: {e.Message}");
        }
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isPasswordValid ? user : null;
        }
        catch (Exception e)
        {
            throw new Exception($"Error authenticating user: {e.Message}");
        }
    }

    public async Task<User?> GetUserAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user != null)
            {
                user.AssignedTasks = (await _taskRepository.GetByAssignedUserIdAsync(id)).ToList();
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

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordHash = newPasswordHash;

            return await _userRepository.UpdateAsync(user);
        }
        catch (Exception e)
        {
            throw new Exception($"Error changing password: {e.Message}");
        }
    }
}