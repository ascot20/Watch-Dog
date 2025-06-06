using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Models;
using WatchDog.Services;
using Task = WatchDog.Models.Task;

namespace WatchDog.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected readonly IAuthorizationService? AuthorizationService;

    public ViewModelBase(IAuthorizationService authorizationservice)
    {
        AuthorizationService = authorizationservice;
    }

    public ViewModelBase()
    {
        
    }
    
    [RelayCommand]
    protected void NavigateToDashboard()
    {
       Navigator.Navigate<DashboardViewModel>(); 
    }
   
    [RelayCommand]
    protected void NavigateToNewProject()
    {
       Navigator.Navigate<NewProjectViewModel>(); 
    }
    
    [RelayCommand]
    protected void NavigateToLogin()
    {
        Navigator.Navigate<LoginViewModel>();
    }

    [RelayCommand]
    protected void NavigateToProject(Project? project)
    {
        if (project != null)
        {
            Navigator.NavigateWithParameter<ProjectViewModel>(project.Id);
        }
    }

    [RelayCommand]
    protected void NavigateToTask(Task? task)
    {
        if (task != null)
        {
            Navigator.NavigateWithParameter<TaskViewModel>(task.Id);
        }
    }

    [RelayCommand]
    protected void NavigateToRegister()
    {
        Navigator.Navigate<RegisterUserViewModel>();
    }

    [RelayCommand]
    protected async System.Threading.Tasks.Task LogoutAsync()
    {
        try
        {
            if (AuthorizationService == null)
            {
                NavigateToLogin();
                return;
            }

            await AuthorizationService.LogoutAsync();
            NavigateToLogin();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error logging out: {e.Message}");
            NavigateToLogin();
        }
    }

    
}
