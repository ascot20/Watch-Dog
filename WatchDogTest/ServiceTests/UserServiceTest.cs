using Moq;
using WatchDog.Data.Repositories;
using WatchDog.Models;
using WatchDog.Services;
using Task = System.Threading.Tasks.Task;

namespace WatchDogTest.ServiceTests;

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<IUserProjectRepository> _mockUserProjectRepository;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly UserService _userService;

    public UserServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTaskService = new Mock<ITaskService>();
        _mockUserProjectRepository = new Mock<IUserProjectRepository>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockTaskService.Object,
            _mockUserProjectRepository.Object,
            _mockAuthorizationService.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        string email = "test@example.com";
        string username = "testuser";
        string password = "password123";
        UserRole role = UserRole.User;
        int expectedUserId = 1;

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(expectedUserId);

        // Act
        int result = await _userService.RegisterAsync(email, username, password, role);

        // Assert
        Assert.Equal(expectedUserId, result);
        _mockUserRepository.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Email == email &&
            u.Username == username &&
            u.Role == role &&
            !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        string email = "test@example.com";
        string username = "testuser";
        string password = "";
        UserRole role = UserRole.User;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.RegisterAsync(email, username, password, role));
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        string email = "";
        string username = "testuser";
        string password = "password123";
        UserRole role = UserRole.User;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.RegisterAsync(email, username, password, role));
    }

    [Fact]
    public async Task RegisterAsync_WithNonAdminUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        string email = "test@example.com";
        string username = "testuser";
        string password = "password123";
        UserRole role = UserRole.User;

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _userService.RegisterAsync(email, username, password, role));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        string email = "test@example.com";
        string username = "testuser";
        string password = "password123";
        UserRole role = UserRole.User;

        var existingUser = new User
            { Id = 1, Email = email, Username = "existing", PasswordHash = "password", Role = UserRole.User };

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _userService.RegisterAsync(email, username, password, role));
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        string email = "test@example.com";
        var expectedUser = new User
            { Id = 1, Email = email, Username = "testuser", PasswordHash = "password", Role = UserRole.User };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        Assert.Equal(expectedUser, result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.GetUserByEmailAsync(""));
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        string email = "nonexistent@example.com";
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region AuthenticateAsync Tests

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        string email = "test@example.com";
        string password = "password123";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = email,
            Username = "testuser",
            PasswordHash = hashedPassword,
            Role = UserRole.User
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

        // Act
        var result = await _userService.AuthenticateAsync(email, password);

        // Assert
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        string email = "test@example.com";
        string password = "wrongpassword";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");

        var user = new User
        {
            Id = 1,
            Email = email,
            Username = "testuser",
            PasswordHash = hashedPassword,
            Role = UserRole.User
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

        // Act
        var result = await _userService.AuthenticateAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "password123";

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.AuthenticateAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("test@example.com", "")]
    [InlineData(null, "password123")]
    [InlineData("test@example.com", null)]
    public async Task AuthenticateAsync_WithEmptyCredentials_ShouldThrowArgumentException(string email, string password)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.AuthenticateAsync(email, password));
    }

    #endregion

    #region GetUserAsync Tests

    [Fact]
    public async Task GetUserAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        int userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SearchUsersAsync Tests

    [Fact]
    public async Task SearchUsersAsync_WithValidSearchTerm_ShouldReturnMatchingUsers()
    {
        // Arrange
        string searchTerm = "test";
        var expectedUsers = new List<User>
        {
            new() { Id = 1, Username = "testuser1", Email = "email1", PasswordHash = "password", Role = UserRole.User },
            new() { Id = 2, Username = "testuser2", Email = "email", PasswordHash = "password", Role = UserRole.User },
        };

        _mockUserRepository.Setup(x => x.SearchAsync(searchTerm)).ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.SearchUsersAsync(searchTerm);

        // Assert
        Assert.Equal(expectedUsers.Count, result.Count());
        Assert.Equal(expectedUsers, result);
    }

    [Fact]
    public async Task SearchUsersAsync_WithEmptySearchTerm_ShouldReturnAllUsers()
    {
        // Arrange
        string searchTerm = "";
        var allUsers = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "email1", PasswordHash = "password", Role = UserRole.User },
            new() { Id = 2, Username = "user2", Email = "email2", PasswordHash = "password", Role = UserRole.User },
            new() { Id = 3, Username = "user3", Email = "email3", PasswordHash = "password", Role = UserRole.User }
        };

        _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allUsers);

        // Act
        var result = await _userService.SearchUsersAsync(searchTerm);

        // Assert
        Assert.Equal(allUsers.Count, result.Count());
        Assert.Equal(allUsers, result);
    }

    #endregion

    #region GetUserNameAsync Tests

    [Fact]
    public async Task GetUserNameAsync_WithValidId_ShouldReturnUsername()
    {
        // Arrange
        int userId = 1;
        string expectedUsername = "testuser";
        var user = new User
        {
            Id = userId, Username = expectedUsername, Email = "example", PasswordHash = "password", Role = UserRole.User
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserNameAsync(userId);

        // Assert
        Assert.Equal(expectedUsername, result);
    }

    [Fact]
    public async Task GetUserNameAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        int userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.GetUserNameAsync(userId));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var expectedUsers = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "email1", PasswordHash = "password", Role = UserRole.User },
            new() { Id = 2, Username = "user2", Email = "email2", PasswordHash = "password", Role = UserRole.User }
        };

        _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        Assert.Equal(expectedUsers.Count, result.Count());
        Assert.Equal(expectedUsers, result);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ShouldChangePassword()
    {
        // Arrange
        int userId = 1;
        string oldPassword = "oldpassword";
        string newPassword = "newpassword";
        string oldHashedPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword);

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = oldHashedPassword,
            Role = UserRole.User,
            Email = "email"
        };

        User? updatedUser = null; // Capture the updated user

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u) // Capture the updated user
            .ReturnsAsync(true);

        // Act
        var result = await _userService.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        Assert.True(result);

        // Verify the password was updated correctly
        Assert.NotNull(updatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, updatedUser.PasswordHash));

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidOldPassword_ShouldReturnFalse()
    {
        // Arrange
        int userId = 1;
        string oldPassword = "wrongpassword";
        string newPassword = "newpassword";
        string correctOldPassword = "correctoldpassword";
        string oldHashedPassword = BCrypt.Net.BCrypt.HashPassword(correctOldPassword);

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = oldHashedPassword,
            Role = UserRole.User,
            Email = "email"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        Assert.False(result);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("", "newpassword")]
    [InlineData("oldpassword", "")]
    [InlineData(null, "newpassword")]
    [InlineData("oldpassword", null)]
    public async Task ChangePasswordAsync_WithEmptyPasswords_ShouldThrowArgumentException(string oldPassword,
        string newPassword)
    {
        // Arrange
        int userId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.ChangePasswordAsync(userId, oldPassword, newPassword));
    }

    [Fact]
    public async Task ChangePasswordAsync_WithSamePasswords_ShouldThrowArgumentException()
    {
        // Arrange
        int userId = 1;
        string password = "samepassword";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.ChangePasswordAsync(userId, password, password));
    }

    #endregion

}