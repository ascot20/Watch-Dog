using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Services;

namespace WatchDog.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;

    [ObservableProperty] private string _email = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private string _buttonText = "Sign In";

    public LoginViewModel(IUserService userService, 
        ISessionService sessionService)
    {
        _userService = userService;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required";
            IsLoading = false;
            return;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Incorrect email format.";
            IsLoading = false;
            return;
        }

        try
        {
            var user = await _userService.AuthenticateAsync(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password";
                IsLoading = false;
                return;
            }

            var fullUser = await _userService.GetUserAsync(user.Id);
            if (fullUser == null)
            {
                ErrorMessage = "Error retrieving user";
                IsLoading = false;
                return;
            }

            _sessionService.SetCurrentUser(fullUser);
            NavigateToDashboard();
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
            Console.WriteLine(e);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnIsLoadingChanged(bool value)
    {
        ButtonText = value ? "Signing In..." : "Sign In";
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