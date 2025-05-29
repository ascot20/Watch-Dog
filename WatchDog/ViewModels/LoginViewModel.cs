using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WatchDog.ViewModels;

public partial class LoginViewModel:ObservableObject
{
    [ObservableProperty] 
    private string _email = string.Empty;
    
    [ObservableProperty] 
    private string _password = string.Empty;

    [ObservableProperty] 
    private bool _isLoading;
    
    [ObservableProperty] 
    private string _errorMessage = string.Empty;
    
    [ObservableProperty] 
    private string _buttonText = "Sign In";

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsLoading = true;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required";
            return;
        }

        try
        {
            
            await Task.Delay(1000);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    partial void OnIsLoadingChanged(bool value)
    {
        ButtonText = value ? "Signing In..." : "Sign In";
    }

}