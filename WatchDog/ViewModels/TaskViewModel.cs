using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Models;
using WatchDog.Services;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.ViewModels;

public partial class TaskViewModel : ViewModelBase, IInitializable
{
    private readonly ITaskService _taskService;
    private readonly ISubtaskService _subtaskService;
    private readonly IProgressionMessageService _progressionMessageService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty] private Models.Task? _task;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private string _newSubtask = string.Empty;
    [ObservableProperty] private string _newProgressionMessage = string.Empty;
    [ObservableProperty] private string _newRemarks = string.Empty;

    [ObservableProperty] private ObservableCollection<SubTask> _subtasks = new();
    [ObservableProperty] private ObservableCollection<ProgressionMessage> _progressionMessages = new();


    public TaskViewModel(
        ITaskService taskService,
        ISubtaskService subtaskService,
        IProgressionMessageService progressionMessageService,
        IAuthorizationService authorizationService
    ):base(authorizationService)
    {
        _taskService = taskService;
        _subtaskService = subtaskService;
        _progressionMessageService = progressionMessageService;
        _authorizationService = authorizationService;
    }

    public async Task InitializeAsync(object parameter)
    {
        if (parameter is int taskId)
        {
            await LoadTaskAsync(taskId);
        }
    }


    private async Task LoadTaskAsync(int taskId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Task = await _taskService.GetTaskAsync(taskId);

            if (Task == null)
            {
                ErrorMessage = "Task not found";
                return;
            }

            Subtasks = new ObservableCollection<SubTask>(Task.SubTasks);
            ProgressionMessages = new ObservableCollection<ProgressionMessage>(Task.ProgressionMessages);
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
    private async Task AddSubTaskAsync()
    {
        try
        {
            Helper.ValidateString(NewSubtask);

            IsLoading = true;
            ErrorMessage = string.Empty;

            int subtaskId = await _subtaskService.CreateSubtaskAsync(
                description: NewSubtask,
                taskId: Task!.Id,
                creatorId: _authorizationService.GetCurrentUserId());

            if (subtaskId > 0)
            {
                await UpdateTaskCompletion();
                await LoadTaskAsync(Task.Id);
                NewSubtask = string.Empty;
            }
            else
            {
                ErrorMessage = "Failed to add subtask";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to add subTask";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateRemarksAsync()
    {
        if (Task == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            bool success = await _taskService.UpdateTaskAsync(
                taskId: Task.Id,
                remarks: NewRemarks);

            if (success)
            {
                await LoadTaskAsync(Task.Id);
                NewRemarks = string.Empty;
            }
            else
            {
                ErrorMessage = "Failed to update remarks";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to update remarks";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddProgressionMessageAsync()
    {
        try
        {
            Helper.ValidateString(NewProgressionMessage);

            IsLoading = true;
            ErrorMessage = string.Empty;

            int messageId = await _progressionMessageService.CreateMessageAsync(
                message: NewProgressionMessage,
                taskId: Task!.Id,
                creatorId: _authorizationService.GetCurrentUserId());

            if (messageId > 0)
            {
                await LoadTaskAsync(Task.Id);
                NewProgressionMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "Failed to add progression message";
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
    private async Task DeleteSubtaskAsync(SubTask? subTask)
    {
        if (subTask == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            bool success = await _subtaskService.DeleteSubtaskAsync(subTask.Id);

            if (success)
            {
                Subtasks.Remove(subTask);
                await UpdateTaskCompletion();
                await LoadTaskAsync(Task!.Id);
            }
            else
            {
                ErrorMessage = "Failed to delete subtask";
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
    private async Task UpdateSubTaskStatusAsync(SubTask? subTask)
    {
        if (subTask == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            bool success = await _subtaskService.UpdateSubtaskAsync(
                subTaskId: subTask.Id,
                isCompleted: subTask.IsComplete);

            if (!success)
            {
                ErrorMessage = "Failed to update subtask status";

                subTask.IsComplete = !subTask.IsComplete;
            }
            else
            {
                await UpdateTaskCompletion();
                await LoadTaskAsync(Task!.Id);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Error updating subtask status";
            subTask.IsComplete = !subTask.IsComplete;
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task UpdateTaskCompletion()
    {
        if (Task == null) return;

        if (Subtasks.Count == 0)
        {
            Task.PercentageComplete = 0;
        }
        else
        {
            int completedSubtasks = Subtasks.Count(s => s.IsComplete);

            Task.PercentageComplete = (int)Math.Round((double)completedSubtasks / Subtasks.Count * 100);
        }


        try
        {
            await _taskService.UpdateTaskAsync(
                taskId: Task.Id,
                percentageCompleted: Task.PercentageComplete);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}