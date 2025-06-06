using Moq;
using WatchDog.Data.Repositories;
using WatchDog.Models;
using WatchDog.Services;
using Task = System.Threading.Tasks.Task;

namespace WatchDogTest.ServiceTests;

public class ProjectServiceTest
{
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IUserProjectRepository> _mockUserProjectRepository;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<ITimeLineMessageService> _mockTimeLineMessageService;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly ProjectService _projectService;

    public ProjectServiceTest()
    {
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockUserProjectRepository = new Mock<IUserProjectRepository>();
        _mockUserService = new Mock<IUserService>();
        _mockTaskService = new Mock<ITaskService>();
        _mockTimeLineMessageService = new Mock<ITimeLineMessageService>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();

        _projectService = new ProjectService(
            _mockProjectRepository.Object,
            _mockUserProjectRepository.Object,
            _mockUserService.Object,
            _mockTaskService.Object,
            _mockTimeLineMessageService.Object,
            _mockAuthorizationService.Object
        );
    }

    #region CreateProjectAsync Tests

    [Fact]
    public async Task CreateProjectAsync_WithValidInputsAndAdminAccess_ShouldCreateProject()
    {
        string title = "Test Project";
        string description = "This is a test project";
        int expectedProjectId = 1;
        int currentUserId = 1;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId())
            .Returns(currentUserId);
        _mockProjectRepository.Setup(repo => repo.CreateAsync(It.IsAny<Project>()))
            .ReturnsAsync(expectedProjectId);

        _mockTimeLineMessageService
            .Setup(m => m.CreateMessageAsync(
                It.IsAny<string>(),
                currentUserId,
                expectedProjectId,
                MessageType.Announcement,
                true))
            .ReturnsAsync(1);

        int result = await _projectService.CreateProjectAsync(title, description);

        Assert.Equal(expectedProjectId, result);
        _mockProjectRepository.Verify(repo => repo.CreateAsync(It.Is<Project>(p =>
            p.Title == title &&
            p.Description == description &&
            p.Status == ProjectStatus.NotStarted)), Times.Once);

        _mockTimeLineMessageService.Verify(
            m => m.CreateMessageAsync(
                $"Project '{title}' has been created",
                currentUserId,
                expectedProjectId,
                MessageType.Announcement,
                true),
            Times.Once);
    }

    [Fact]
    public async Task CreateProjectAsync_WithoutAdminAccess_ShouldThrowUnauthorizedException()
    {
        string title = "Test Project";
        string description = "This is a test project";

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.CreateProjectAsync(title, description));
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData(null, "Description")]
    [InlineData("   ", "Description")]
    public async Task CreateProjectAsync_WithInvalidTitle_ShouldThrowArgumentException(
        string title, string description)
    {
        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectService.CreateProjectAsync(title, description));
    }

    #endregion

    #region GetProjectAsync Tests

    [Fact]
    public async Task GetProjectAsync_WithValidId_ShouldReturnProject()
    {
        int projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Title = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.InProgress
        };

        var tasks = new List<WatchDog.Models.Task>
        {
            new WatchDog.Models.Task
            {
                Id = 1,
                TaskDescription = "Task 1",
                ProjectId = projectId,
                AssignedUserId = 1
            }
        };

        var messages = new List<TimeLineMessage>
        {
            new TimeLineMessage
            {
                Id = 1,
                Content = "Project created",
                ProjectId = projectId,
                AuthorId = 1,
                Type = MessageType.Update
            }
        };

        var members = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "user1",
                Email = "user1@example.com",
                PasswordHash = "hash",
                Role = UserRole.User
            }
        };

        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _mockTaskService.Setup(service => service.GetByProjectIdAsync(projectId))
            .ReturnsAsync(tasks);
        _mockTimeLineMessageService.Setup(service => service.GetMessagesByProjectIdAsync(projectId))
            .ReturnsAsync(messages);
        _mockUserProjectRepository.Setup(repo => repo.GetUsersByProjectIdAsync(projectId))
            .ReturnsAsync(members);

        var result = await _projectService.GetProjectAsync(projectId);

        Assert.NotNull(result);
        Assert.Equal(projectId, result.Id);
        Assert.Single(result.Tasks);
        Assert.Single(result.TimeLineMessages);
        Assert.Single(result.ProjectMembers);
    }

    [Fact]
    public async Task GetProjectAsync_WithInvalidId_ShouldReturnNull()
    {
        int projectId = 999;

        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync((Project)null);

        var result = await _projectService.GetProjectAsync(projectId);

        Assert.Null(result);
    }

    #endregion

    #region UpdateProjectAsync Tests

    [Fact]
    public async Task UpdateProjectAsync_WithValidInputsAndAdminAccess_ShouldUpdateProject()
    {
        int projectId = 1;
        int currentUserId = 1;
        var project = new Project
        {
            Id = projectId,
            Title = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.InProgress
        };

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId())
            .Returns(currentUserId);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _mockProjectRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Project>()))
            .ReturnsAsync(true);

        _mockTimeLineMessageService
            .Setup(m => m.CreateMessageAsync(
                It.IsAny<string>(),
                currentUserId,
                projectId,
                MessageType.Milestone,
                false))
            .ReturnsAsync(1);

        bool result = await _projectService.UpdateProjectAsync(projectId, ProjectStatus.Completed);

        Assert.True(result);

        _mockTimeLineMessageService.Verify(
            m => m.CreateMessageAsync(
                $"Project '{project.Title}' has been marked as completed",
                currentUserId,
                projectId,
                MessageType.Milestone,
                false),
            Times.Once);
    }


    [Fact]
    public async Task UpdateProjectAsync_WithoutAdminAccess_ShouldThrowUnauthorizedException()
    {
        int projectId = 1;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.UpdateProjectAsync(projectId, ProjectStatus.Completed));
    }

    #endregion

    #region DeleteProjectAsync Tests

    [Fact]
    public async Task DeleteProjectAsync_WithValidIdAndAdminAccess_ShouldDeleteProject()
    {
        int projectId = 1;
        var project = new Project
        {
            Id = projectId,
            Title = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.InProgress
        };

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _mockProjectRepository.Setup(repo => repo.DeleteAsync(projectId))
            .Returns(Task.CompletedTask);

        bool result = await _projectService.DeleteProjectAsync(projectId);

        Assert.True(result);
        _mockProjectRepository.Verify(repo => repo.DeleteAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_WithoutAdminAccess_ShouldThrowUnauthorizedException()
    {
        int projectId = 1;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.DeleteProjectAsync(projectId));
    }

    [Fact]
    public async Task DeleteProjectAsync_WithInvalidProjectId_ShouldThrowArgumentException()
    {
        int projectId = 999;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync((Project)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectService.DeleteProjectAsync(projectId));
        Assert.Contains("Project with ID 999 does not exist", exception.Message);
    }

    #endregion

    #region AddUserToProjectAsync Tests

    [Fact]
    public async Task AddUserToProjectAsync_WithValidInputsAndAdminAccess_ShouldAddUser()
    {
        int projectId = 1;
        int userId = 2;
        int currentUserId = 1;
        string username = "testuser";

        var project = new Project
        {
            Id = projectId,
            Title = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.InProgress
        };

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId())
            .Returns(currentUserId);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _mockUserProjectRepository.Setup(repo => repo.AddAsync(userId, projectId))
            .ReturnsAsync(true);
        _mockUserService.Setup(service => service.GetUserNameAsync(userId))
            .ReturnsAsync(username);

        _mockTimeLineMessageService
            .Setup(m => m.CreateMessageAsync(
                It.IsAny<string>(),
                currentUserId,
                projectId,
                MessageType.Update,
                It.IsAny<bool>()))
            .ReturnsAsync(1);

        bool result = await _projectService.AddUserToProjectAsync(projectId, userId);

        Assert.True(result);

        _mockTimeLineMessageService.Verify(
            m => m.CreateMessageAsync(
                $"{username} has been added to the project",
                currentUserId,
                projectId,
                MessageType.Update,
                It.IsAny<bool>()),
            Times.Once);
    }


    [Fact]
    public async Task AddUserToProjectAsync_WithoutAdminAccess_ShouldThrowUnauthorizedException()
    {
        int projectId = 1;
        int userId = 2;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.AddUserToProjectAsync(projectId, userId));
    }

    [Fact]
    public async Task AddUserToProjectAsync_WithInvalidProjectId_ShouldThrowArgumentException()
    {
        int projectId = 999;
        int userId = 2;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync((Project)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectService.AddUserToProjectAsync(projectId, userId));
        
        Assert.Contains("Project with ID 999 does not exist", exception.Message);
    }

    #endregion

    #region RemoveUserFromProjectAsync Tests

    [Fact]
    public async Task RemoveUserFromProjectAsync_WithValidInputsAndAdminAccess_ShouldRemoveUser()
    {
        int projectId = 1;
        int userId = 2;
        int currentUserId = 1;

        var project = new Project
        {
            Id = projectId,
            Title = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.InProgress
        };

        var tasks = new List<WatchDog.Models.Task>
        {
            new WatchDog.Models.Task
            {
                Id = 1,
                TaskDescription = "Task 1",
                ProjectId = projectId,
                AssignedUserId = userId
            }
        };

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockAuthorizationService.Setup(auth => auth.GetCurrentUserId())
            .Returns(currentUserId);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync(project);
        _mockTaskService.Setup(service => service.GetByAssignedUserIdAsync(userId))
            .ReturnsAsync(tasks);
        _mockTaskService.Setup(service => service.UpdateTaskAsync(
                It.IsAny<int>(),      
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))    
            .ReturnsAsync(true);
 
        _mockUserProjectRepository.Setup(repo => repo.RemoveAsync(projectId, userId))
            .ReturnsAsync(true);

        _mockTimeLineMessageService
            .Setup(m => m.CreateMessageAsync(
                It.IsAny<string>(),
                currentUserId,
                
                projectId,
                MessageType.Update,
                It.IsAny<bool>()))
            .ReturnsAsync(1);

        bool result = await _projectService.RemoveUserFromProjectAsync(projectId, userId);

        Assert.True(result);
        _mockTaskService.Verify(service => service.UpdateTaskAsync(
            It.IsAny<int>(),
            null,
            null,
            currentUserId), Times.Once);
        _mockUserProjectRepository.Verify(repo => repo.RemoveAsync(projectId, userId), Times.Once);

        _mockTimeLineMessageService.Verify(
            m => m.CreateMessageAsync(
                "A team member has been removed from the project",
                currentUserId,
                
                projectId,
                MessageType.Update,
                It.IsAny<bool>()),
            Times.Once);
    }


    [Fact]
    public async Task RemoveUserFromProjectAsync_WithoutAdminAccess_ShouldThrowUnauthorizedException()
    {
        int projectId = 1;
        int userId = 2;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.RemoveUserFromProjectAsync(projectId, userId));
    }

    [Fact]
    public async Task RemoveUserFromProjectAsync_WithInvalidProjectId_ShouldThrowArgumentException()
    {
        int projectId = 999;
        int userId = 2;

        _mockAuthorizationService.Setup(auth => auth.IsAdmin())
            .Returns(true);
        _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId))
            .ReturnsAsync((Project)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _projectService.RemoveUserFromProjectAsync(projectId, userId));
        
        Assert.Contains("Project with ID 999 does not exist", exception.Message);
    }

    #endregion

    #region GetAllProjectsAsync Tests

    [Fact]
    public async Task GetAllProjectsAsync_ShouldReturnAllProjects()
    {
        var projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Title = "Project 1",
                Description = "Description 1",
                Status = ProjectStatus.NotStarted
            },
            new Project
            {
                Id = 2,
                Title = "Project 2",
                Description = "Description 2",
                Status = ProjectStatus.InProgress
            }
        };

        _mockProjectRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(projects);

        var result = await _projectService.GetAllProjectsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, ((IEnumerable<Project>)result).Count());
    }


    #endregion
} 
