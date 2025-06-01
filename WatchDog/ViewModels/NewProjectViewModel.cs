using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Models;
using WatchDog.Services;

namespace WatchDog.ViewModels;

public partial class NewProjectViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly ISessionService _sessionService;
    private readonly ITaskService _taskService;

    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private ObservableCollection<User> _searchResults = new();
    [ObservableProperty] private User? _selectedUser;
    [ObservableProperty] private ObservableCollection<TeamMemberTaskViewModel> _teamMembers = new();
    [ObservableProperty] private string _projectName = string.Empty;
    [ObservableProperty] private string _projectDescription = string.Empty;
    [ObservableProperty] private string _taskDescription = string.Empty;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _hasSearchResults = false;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private User? _currentUser;
    [ObservableProperty] private User? _pendingUser;

    public NewProjectViewModel(IUserService userService, IProjectService projectService,
        ISessionService sessionService, ITaskService taskService) 
    {
        _userService = userService;
        _projectService = projectService;
        _sessionService = sessionService;
        _taskService = taskService;

        LoadCurrentUser();
    }

    private void LoadCurrentUser()
    {
        try
        {
            CurrentUser = _sessionService.GetCurrentUser();
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }


    [RelayCommand]
    private async System.Threading.Tasks.Task SearchUsersAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var results = await _userService.SearchUsersAsync(SearchTerm);
            var filteredResults = results.Where(user =>
                TeamMembers.All(tm => tm.Member.Id != user.Id)).ToList();

            SearchResults = new ObservableCollection<User>(filteredResults);
            HasSearchResults = SearchResults.Count > 0;

            if (!HasSearchResults)
            {
                ErrorMessage = "No users found";
            }
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectUserForTask()
    {
        if (SelectedUser == null) return;

        PendingUser = SelectedUser;

        SelectedUser = null;
        SearchResults.Clear();
        HasSearchResults = false;
        SearchTerm = string.Empty;

        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void AddUserWithTask()
    {
        if (PendingUser == null)
        {
            ErrorMessage = "Please select a user first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TaskDescription))
        {
            ErrorMessage = "Task description is required to add a team member.";
            return;
        }

        var teamMember = new TeamMemberTaskViewModel
        {
            Member = PendingUser,
            TaskDescriptions = new ObservableCollection<string> { TaskDescription }
        };

        TeamMembers.Add(teamMember);

        PendingUser = null;
        TaskDescription = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void RemoveUserFromTeam(TeamMemberTaskViewModel teamMember)
    {
        if (teamMember != null)
        {
            TeamMembers.Remove(teamMember);
        }
    }

    [RelayCommand]
    private void AddTaskToTeamMember(TeamMemberTaskViewModel teamMember)
    {
        if (string.IsNullOrWhiteSpace(TaskDescription))
        {
            ErrorMessage = "Task description cannot be empty.";
            return;
        }

        if (teamMember != null)
        {
            teamMember.TaskDescriptions.Add(TaskDescription);

            TaskDescription = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
    
    [RelayCommand]
    private void RemoveTask(string taskDescription)
    {
        foreach (var teamMember in TeamMembers)
        {
            if (teamMember.TaskDescriptions.Contains(taskDescription))
            {
                teamMember.TaskDescriptions.Remove(taskDescription);
               
                //if this was the last task, remove the team member
                if (teamMember.TaskDescriptions.Count == 0)
                {
                    TeamMembers.Remove(teamMember);
                }
                
                break;
            }
        }
    }
    
    [RelayCommand]
    private async System.Threading.Tasks.Task CreateProjectAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ErrorMessage = "Project name is required.";
            return;
        }

        if (TeamMembers.Count == 0)
        {
            ErrorMessage = "At least one team member with a task is required.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            
            // Call the project service to create the project
            int projectId = await _projectService.CreateProjectAsync(ProjectName, ProjectDescription);
            
            // Add the current user and all team members to the project
            await _projectService.AddUserToProjectAsync(projectId, CurrentUser.Id);
              
            foreach (var teamMember in TeamMembers)
            {
                await _projectService.AddUserToProjectAsync( projectId,teamMember.Member.Id);
                
                // Create tasks for this team member
                foreach (var taskDescription in teamMember.TaskDescriptions)
                {
                    await _taskService.CreateTaskAsync(taskDescription,projectId,teamMember.Member.Id);
                }
            }
            
            NavigateToDashboard();
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void CancelAddUser()
    {
        PendingUser = null;
        TaskDescription = string.Empty;
        ErrorMessage = string.Empty;
    }


}