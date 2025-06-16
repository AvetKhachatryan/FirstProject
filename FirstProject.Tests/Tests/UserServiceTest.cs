using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;
using FirstProject.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FirstProject.Tests.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepo> _repoMock;
        private readonly Mock<IConfiguration> configMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _repoMock = new Mock<IUserRepo>();
            configMock = new Mock<IConfiguration>();
            _service = new UserService(_repoMock.Object, configMock.Object);
        }

        [Fact]
        public void GetByUsername_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var testUser = new User { Id = 1, Username = "testuser" };
            _repoMock.Setup(r => r.GetUsers()).Returns(new List<User> { testUser });

            // Act
            var result = _service.GetByUsername("testuser");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testUser);
        }

        [Fact]
        public void GetByUsername_ShouldReturnNull_WhenUserNotExists()
        {
            // Arrange
            _repoMock.Setup(r => r.GetUsers()).Returns(new List<User>());

            // Act
            var result = _service.GetByUsername("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddUserAsync_ShouldAddUser_WhenUsernameNotExists()
        {
            // Arrange
            var users = new List<User>();
            _repoMock.Setup(r => r.GetUsers()).Returns(users);
            _repoMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask)
                .Callback<User>(u => users.Add(u));

            string username = "newuser";
            string password = "password";
            string passwordConfirm = "password";
            string email = "email@example.com";

            // Act
            var result = await _service.AddUserAsync(username, password, passwordConfirm, email);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(username);
            users.Should().Contain(u => u.Username == username);
            _repoMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task AddUserAsync_ShouldThrowException_WhenUsernameExists()
        {
            // Arrange
            var existingUser = new User { Username = "existinguser" };
            var users = new List<User> { existingUser };
            _repoMock.Setup(r => r.GetUsers()).Returns(users);

            // Act
            Func<Task> act = async () =>
            {
                await _service.AddUserAsync("existinguser", "pass", "pass", "email@example.com");
            };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("User with this username already exists");
            _repoMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void GeneratePasswordResetToken_ValidEmail_ReturnsToken()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ResetPasswordJwt:Key"]).Returns("a-string-secret-at-least-256-bits-long");
            configMock.Setup(c => c["ResetPasswordJwt:Issuer"]).Returns("TestIssuer");
            configMock.Setup(c => c["ResetPasswordJwt:Audience"]).Returns("TestAudience");
            configMock.Setup(c => c["ResetPasswordJwt:ExpiresInMinutes"]).Returns("60");

            var repoMock = new Mock<IUserRepo>();
            var service = new UserService(repoMock.Object, configMock.Object);

            string testEmail = "test@example.com";

            // Act
            string token = service.GeneratePasswordResetToken(testEmail);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value.Should().Be(testEmail);
            jwt.Issuer.Should().Be("TestIssuer");
            jwt.Audiences.Should().Contain("TestAudience");
        }

    }

}