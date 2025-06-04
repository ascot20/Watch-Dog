using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Models;
using WatchDog.Services;

namespace WatchDog.ViewModels;

public partial class ProjectViewModel : ViewModelBase, IInitializable
{
    private readonly IProjectService _projectService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ITimeLineMessageService _timeLineMessageService;
    private int _projectId;

    [ObservableProperty] private Project _project;
    [ObservableProperty] private int _percentageCompleted;
    [ObservableProperty] private ObservableCollection<User> _teamMembers = new();
    [ObservableProperty] private ObservableCollection<Models.Task> _projectTasks = new();
    [ObservableProperty] private ObservableCollection<TimeLineMessage> _timelineMessages = new();
    [ObservableProperty] private ObservableCollection<MessageType> _messageTypes;
    [ObservableProperty] private MessageType _selectedMessageType = MessageType.Question;
    [ObservableProperty] private string _newMessage = string.Empty;
    [ObservableProperty] private bool _isUpdateStatusDropdownOpen = false;
    [ObservableProperty] private ProjectStatus _selectedStatus;

    [ObservableProperty] private ObservableCollection<ProjectStatus> _availableStatuses;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage;
    [ObservableProperty] private bool _isAdmin;

    public IEnumerable<TimeLineMessage> PinnedMessages =>
        TimelineMessages.Where(m => m.IsPinned).ToList();

    public bool HasPinnedMessages => PinnedMessages.Any();

    public ProjectViewModel(
        IProjectService projectService,
        IAuthorizationService authorizationService,
        ITimeLineMessageService timeLineMessageService
    )
    {
        _projectService = projectService;
        _authorizationService = authorizationService;
        _timeLineMessageService = timeLineMessageService;

        _messageTypes = new ObservableCollection<MessageType>(
            Enum.GetValues(typeof(MessageType)).Cast<MessageType>());

        _availableStatuses = new ObservableCollection<ProjectStatus>(
            Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>());

        IsAdmin = _authorizationService.IsAdmin();
    }

    public async System.Threading.Tasks.Task InitializeAsync(object parameter)
    {
        if (parameter is int projectId)
        {
            _projectId = projectId;
            await LoadProjectAsync();
        }
    }

    private async System.Threading.Tasks.Task LoadProjectAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            Project = await _projectService.GetProjectAsync(_projectId);

            if (Project == null)
            {
                ErrorMessage = "Project not found";
                return;
            }

            TeamMembers = new ObservableCollection<User>(Project.ProjectMembers);
            ProjectTasks = new ObservableCollection<Models.Task>(Project.Tasks);
            TimelineMessages = new ObservableCollection<TimeLineMessage>(Project.TimeLineMessages);

            SelectedStatus = Project.Status;

            CalculateProjectCompletion();
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

    private void CalculateProjectCompletion()
    {
        if (ProjectTasks.Count == 0)
        {
            PercentageCompleted = 0;
            return;
        }

        int totalPercentage = ProjectTasks.Sum(t => t.PercentageComplete);
        PercentageCompleted = totalPercentage / ProjectTasks.Count;
    }

    [RelayCommand]
    private void ToggleUpdateStatusDropdown()
    {
        IsUpdateStatusDropdownOpen = !IsUpdateStatusDropdownOpen;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task AddTimeLineMessageAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
            {
                ErrorMessage = "Message cannot be empty";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            int result = await _timeLineMessageService.CreateMessageAsync(
                message: NewMessage,
                projectId: Project.Id,
                creatorId: _authorizationService.GetCurrentUserId(),
                type: SelectedMessageType);

            if (result < 1)
            {
                ErrorMessage = "Failed to add message";
                return;
            }

            await LoadProjectAsync();
            NewMessage = string.Empty;
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
    private async System.Threading.Tasks.Task PinMessageAsync()
    {
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task UnpinMessageAsync()
    {
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task DeleteMessageAsync()
    {
    }
    
    [RelayCommand]
    private async System.Threading.Tasks.Task RemoveTeamMemberAsync(){}

    [RelayCommand]
    private async System.Threading.Tasks.Task UpdateProjectStatusAsync(ProjectStatus status)
    {
        if (Project == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Project.Status = status;
            SelectedStatus = status;

            IsUpdateStatusDropdownOpen = false;

            bool suceess = await _projectService.UpdateProjectAsync(_projectId, status);

            if (!suceess)
            {
                ErrorMessage = "Failed to update project status";
            }
            else
            {
                await LoadProjectAsync();
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

}