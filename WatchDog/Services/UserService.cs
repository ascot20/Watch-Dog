using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class UserService:IUserService
{
   private readonly IUserRepository _userRepository;
   private readonly ILogger<UserService> _logger;

   public UserService(IUserRepository userRepository, ILogger<UserService> logger)
   {
      this._userRepository = userRepository;
      this._logger = logger;
   }

   public Task<int> RegisterAsync(User user, string password)
   {
      throw new System.NotImplementedException();
   }

   public Task<User?> AuthenticateAsync(string email, string password)
   {
      throw new System.NotImplementedException();
   }

   public async Task<User?> GetByIdAsync(int userId)
   {
      try
      {
         var user = await _userRepository.GetByIdAsync(userId);

         return user;
      }
      catch (Exception e)
      {
         Console.Error.WriteLine($"Error retrieving user {userId}: {e.Message}");
         return null;
      }
   }

   public Task<User?> GetByEmailAsync(string email)
   {
      throw new System.NotImplementedException();
   }

   public Task<IEnumerable<User>> GetAllAsync()
   {
      throw new System.NotImplementedException();
   }

   public Task<User?> GetWithAssignedTasksAsync(int userId)
   {
      throw new System.NotImplementedException();
   }

   public Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
   {
      throw new System.NotImplementedException();
   }
   
}