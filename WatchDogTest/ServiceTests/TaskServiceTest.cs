using Moq;
using WatchDog.Data.Repositories;
using WatchDog.Models;
using WatchDog.Services;
using Task = System.Threading.Tasks.Task;

namespace WatchDogTest.ServiceTests;

public class TaskServiceTest
{
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<ISubtaskService> _mockSubtaskService;
    private readonly Mock<IProgressionMessageService> _mockProgressionMessageService;
    private readonly Mock<ITimeLineMessageService> _mockTimeLineMessageService;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly TaskService _taskService;

    public TaskServiceTest()
    {
        _mockTaskRepository = new Mock<ITaskRepository>();
        _mockSubtaskService = new Mock<ISubtaskService>();
        _mockProgressionMessageService = new Mock<IProgressionMessageService>();
        _mockTimeLineMessageService = new Mock<ITimeLineMessageService>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();

        _taskService = new TaskService(
            _mockTaskRepository.Object,
            _mockSubtaskService.Object,
            _mockProgressionMessageService.Object,
            _mockTimeLineMessageService.Object,
            _mockAuthorizationService.Object
        );
    }

    #region CreateTaskAsync Tests

    [Fact]
    public async Task CreateTaskAsync_WithValidInputsAndAdminAccess_ShouldCreateTask()
    {
        // Arrange
        string description = "Test Task";
        int projectId = 1;
        int assignedUserId = 2;
        int currentUserId = 1;
        int expectedTaskId = 5;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId()).Returns(currentUserId);
        _mockTaskRepository.Setup(repo => repo.CreateAsync(It.IsAny<WatchDog.Models.Task>()))
            .ReturnsAsync(expectedTaskId);
        _mockTimeLineMessageService.Setup(service => service.CreateMessageAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<MessageType>(), It.IsAny<bool>()))
            .ReturnsAsync(1);

        // Act
        int result = await _taskService.CreateTaskAsync(description, projectId, assignedUserId);

        // Assert
        Assert.Equal(expectedTaskId, result);
        _mockTaskRepository.Verify(repo => repo.CreateAsync(It.Is<WatchDog.Models.Task>(t =>
            t.TaskDescription == description &&
            t.ProjectId == projectId &&
            t.AssignedUserId == assignedUserId)), Times.Once);

        _mockTimeLineMessageService.Verify(service => service.CreateMessageAsync(
            $"Task '{description}' has been created",
            currentUserId,
            projectId,
            MessageType.Update,
            false), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_WithoutAdminAccess_ShouldThrowUnauthorizedException()
    {
        // Arrange
        string description = "Test Task";
        int projectId = 1;
        int assignedUserId = 2;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin()).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _taskService.CreateTaskAsync(description, projectId, assignedUserId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateTaskAsync_WithInvalidDescription_ShouldThrowArgumentException(string description)
    {
        // Arrange
        int projectId = 1;
        int assignedUserId = 2;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin()).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _taskService.CreateTaskAsync(description, projectId, assignedUserId));
    }

    #endregion

    #region GetTaskAsync Tests

    [Fact]
    public async Task GetTaskAsync_WithValidId_ShouldReturnTaskWithSubtasksAndMessages()
    {
        // Arrange
        int taskId = 1;
        var task = new WatchDog.Models.Task
        {
            Id = taskId,
            TaskDescription = "Test Task",
            ProjectId = 1,
            AssignedUserId = 2
        };

        var subtasks = new List<SubTask>
        {
            new SubTask { Id = 1, TaskId = taskId, Description = "Subtask 1", CreatedById = task.AssignedUserId}
        };

        var progressMessages = new List<ProgressionMessage>
        {
            new ProgressionMessage { Id = 1, TaskId = taskId, Content = "Progress update", AuthorId = task.AssignedUserId}
        };

        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);
        _mockSubtaskService.Setup(service => service.GetSubtasksByTaskIdAsync(taskId))
            .ReturnsAsync(subtasks);
        _mockProgressionMessageService.Setup(service => service.GetByTaskIdAsync(taskId))
            .ReturnsAsync(progressMessages);

        // Act
        var result = await _taskService.GetTaskAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Single(result.SubTasks);
        Assert.Single(result.ProgressionMessages);
    }

    [Fact]
    public async Task GetTaskAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        int taskId = 999;
        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((WatchDog.Models.Task)null);

        // Act
        var result = await _taskService.GetTaskAsync(taskId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdateTaskAsync Tests

    [Fact]
    public async Task UpdateTaskAsync_WithValidParameters_ShouldUpdateTask()
    {
        // Arrange
        int taskId = 1;
        string remarks = "Updated remarks";
        int percentageCompleted = 75;
        int assignedUserId = 3;

        var existingTask = new WatchDog.Models.Task
        {
            Id = taskId,
            TaskDescription = "Test Task",
            ProjectId = 1,
            AssignedUserId = 2,
            PercentageComplete = 50
        };

        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(existingTask);
        _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<WatchDog.Models.Task>()))
            .ReturnsAsync(true);

        // Act
        bool result = await _taskService.UpdateTaskAsync(taskId, remarks, percentageCompleted, assignedUserId);

        // Assert
        Assert.True(result);
        Assert.Equal(remarks, existingTask.Remarks);
        Assert.Equal(percentageCompleted, existingTask.PercentageComplete);
        Assert.Equal(assignedUserId, existingTask.AssignedUserId);
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenTaskCompleted_ShouldCreateTimelineMessage()
    {
        // Arrange
        int taskId = 1;
        int percentageCompleted = 100;
        int currentUserId = 1;

        var existingTask = new WatchDog.Models.Task
        {
            Id = taskId,
            TaskDescription = "Test Task",
            ProjectId = 1,
            AssignedUserId = 2,
            PercentageComplete = 75
        };

        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(existingTask);
        _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<WatchDog.Models.Task>()))
            .ReturnsAsync(true);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId()).Returns(currentUserId);
        _mockTimeLineMessageService.Setup(service => service.CreateMessageAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<MessageType>(), It.IsAny<bool>()))
            .ReturnsAsync(1);

        // Act
        bool result = await _taskService.UpdateTaskAsync(taskId, percentageCompleted: percentageCompleted);

        // Assert
        Assert.True(result);
        _mockTimeLineMessageService.Verify(service => service.CreateMessageAsync(
            $"Task '{existingTask.TaskDescription}' has been completed",
            currentUserId,
            existingTask.ProjectId,
            MessageType.Milestone,
            false), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithInvalidTaskId_ShouldReturnFalse()
    {
        // Arrange
        int taskId = 999;
        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((WatchDog.Models.Task)null);

        // Act
        bool result = await _taskService.UpdateTaskAsync(taskId, "remarks");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetByAssignedUserIdAsync Tests

    [Fact]
    public async Task GetByAssignedUserIdAsync_WithValidUserId_ShouldReturnTasks()
    {
        // Arrange
        int userId = 1;
        var tasks = new List<WatchDog.Models.Task>
        {
            new WatchDog.Models.Task { Id = 1, AssignedUserId = userId, TaskDescription = "example task", ProjectId = 5},
            new WatchDog.Models.Task { Id = 2, AssignedUserId = userId , TaskDescription = "example task 2" ,ProjectId = 5}
        };

        _mockTaskRepository.Setup(repo => repo.GetByAssignedUserIdAsync(userId))
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetByAssignedUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetByProjectIdAsync Tests

    [Fact]
    public async Task GetByProjectIdAsync_WithValidProjectId_ShouldReturnTasksWithSubtasksAndMessages()
    {
        // Arrange
        int projectId = 1;
        var tasks = new List<WatchDog.Models.Task>
        {
            new WatchDog.Models.Task { Id = 1, ProjectId = projectId, TaskDescription = "example task", AssignedUserId = 2 },
            new WatchDog.Models.Task { Id = 2, ProjectId = projectId, TaskDescription = "example task 2", AssignedUserId = 4}
        };

        var subtasks = new List<SubTask>();
        var progressMessages = new List<ProgressionMessage>();

        _mockTaskRepository.Setup(repo => repo.GetByProjectIdAsync(projectId))
            .ReturnsAsync(tasks);
        _mockSubtaskService.Setup(service => service.GetSubtasksByTaskIdAsync(It.IsAny<int>()))
            .ReturnsAsync(subtasks);
        _mockProgressionMessageService.Setup(service => service.GetByTaskIdAsync(It.IsAny<int>()))
            .ReturnsAsync(progressMessages);

        // Act
        var result = await _taskService.GetByProjectIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByProjectIdAsync_WhenNoTasks_ShouldReturnEmpty()
    {
        // Arrange
        int projectId = 1;
        _mockTaskRepository.Setup(repo => repo.GetByProjectIdAsync(projectId))
            .ReturnsAsync((IEnumerable<WatchDog.Models.Task>)null);

        // Act
        var result = await _taskService.GetByProjectIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_WithValidId_ShouldDeleteTaskAndCreateTimelineMessage()
    {
        // Arrange
        int taskId = 1;
        int currentUserId = 1;
        var task = new WatchDog.Models.Task
        {
            Id = taskId,
            TaskDescription = "Test Task",
            ProjectId = 1,
            AssignedUserId = 2,
            
        };

        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);
        _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId)).Returns(Task.CompletedTask);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId()).Returns(currentUserId);
        _mockTimeLineMessageService.Setup(service => service.CreateMessageAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<MessageType>(), It.IsAny<bool>()))
            .ReturnsAsync(1);

        // Act
        bool result = await _taskService.DeleteTaskAsync(taskId);

        // Assert
        Assert.True(result);
        _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId), Times.Once);
        _mockTimeLineMessageService.Verify(service => service.CreateMessageAsync(
            $"Task '{task.TaskDescription}' has been deleted",
            currentUserId,
            task.ProjectId,
            MessageType.Update,
            false), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        int taskId = 999;
        _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((WatchDog.Models.Task)null);

        // Act
        bool result = await _taskService.DeleteTaskAsync(taskId);

        // Assert
        Assert.False(result);
        _mockTaskRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

   
}