using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Models;
using WatchDog.Services;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.ViewModels;

public partial class RegisterUserViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private bool _isUserAdmin = false;
    [ObservableProperty] private bool _isCurrentUserAdmin = false;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess = false;

    public RegisterUserViewModel(IUserService userService,
        IAuthorizationService authorizationService) : base(authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;

        IsCurrentUserAdmin = _authorizationService.IsAdmin();
    }

    [RelayCommand]
    private async Task RegiterUserAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Invalid email format";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                return;
            }

            IsLoading = true;

            var existingUser = await _userService.GetUserByEmailAsync(Email);

            if (existingUser != null)
            {
                ErrorMessage = "User with this email already exists";
                IsLoading = false;
                return;
            }

            UserRole role = IsUserAdmin ? UserRole.SuperAdmin : UserRole.User;
            int userId = await _userService.RegisterAsync(
                email: Email,
                username: Username,
                password: Password,
                role: role
            );

            if (userId > 0)
            {
                IsSuccess = true;

                Username = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                ErrorMessage = "Failed to register user";
            }
        }
        catch (Exception e)
        {
            ErrorMessage = "An error occurred";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}