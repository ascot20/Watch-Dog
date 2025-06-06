using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    [ObservableProperty] private ObservableCollection<Task> _myTasks = new();
    [ObservableProperty] private ObservableCollection<User> _allUsers = new();
    [ObservableProperty] private ObservableCollection<User> _filteredUsers = new();

    [ObservableProperty] private string _currentUsername;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isLoadingUsers;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private User? _selectedUser;

    public DashboardViewModel(IProjectService projectService,
        IUserService userService,
        IAuthorizationService authorizationService):base(authorizationService)
    {
        _projectService = projectService;
        _userService = userService;
        _authorizationService = authorizationService;
        
        IsAdmin = _authorizationService.IsAdmin();
        CurrentUsername = _authorizationService.GetCurrentUserName().ToUpper();

        LoadProjects();
        LoadTasks();
        LoadUsers();
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

            foreach (var project in currentUser!.UserProjects)
            {
                MyProjects.Add(project);
            }

            IsLoading = false;
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load projects";
        }
    }

    private async void LoadTasks()
    {
        try
        {
            int userId = _authorizationService.GetCurrentUserId();
            var currentUser = await _userService.GetUserAsync(userId);

            MyTasks.Clear();

            foreach (var task in currentUser!.AssignedTasks)
            {
                MyTasks.Add(task);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load tasks";
        }
    }

    private async void LoadUsers()
    {
        if (IsAdmin)
        {
            try
            {
                IsLoadingUsers = true;

                var users = await _userService.GetAllAsync();
                AllUsers.Clear();
                AllUsers = new ObservableCollection<User>(users);

                FilterUsers();
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to load users";
            }
            finally
            {
                IsLoadingUsers = false;
            }
        }
    }

    private void FilterUsers()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            FilteredUsers = new ObservableCollection<User>(AllUsers);
            return;
        }

        var term = SearchTerm.ToLower();
        var filteredUsers = AllUsers
            .Where(u =>
                u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term))
            .ToList();
        FilteredUsers = new ObservableCollection<User>(filteredUsers);
    }

    [RelayCommand]
    private void SearchAllUsers()
    {
        FilterUsers();
    }
}