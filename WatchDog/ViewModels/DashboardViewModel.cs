using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WatchDog.Models;
using WatchDog.Services;

namespace WatchDog.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty] private ObservableCollection<Project> _feedProjects = new();

    [ObservableProperty] private ObservableCollection<Project> _myProjects = new();
    
    [ObservableProperty]
    private ObservableCollection<Task> _myTasks = new();

    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isAdmin = false;

    public DashboardViewModel(IProjectService projectService,
        IUserService userService,
        IAuthorizationService authorizationService)
    {
        _projectService = projectService;
        _userService = userService;
        _authorizationService = authorizationService;
        IsAdmin = _authorizationService.IsAdmin();
        
        LoadProjects();
        LoadTasks();
    }

    private async void LoadProjects()
    {
        try
        {
            IsLoading = true;
            
            int userId = _authorizationService.GetCurrentUserId();
            var allProjects = await _projectService.GetAllProjectsAsync();
            var currentUser = await _userService.GetUserAsync(userId);

            FeedProjects.Clear();
            MyProjects.Clear();

            foreach (Project project in allProjects)
            {
                FeedProjects.Add(project);
            }

            foreach (var project in currentUser.UserProjects)
            {
                MyProjects.Add(project);
            }

            IsLoading = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async void LoadTasks()
    {
        try
        {
            int userId = _authorizationService.GetCurrentUserId();
            var currentUser = await _userService.GetUserAsync(userId);
            
            MyTasks.Clear();

            foreach (var task in currentUser.AssignedTasks)
            {
                MyTasks.Add(task);
            } 
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}