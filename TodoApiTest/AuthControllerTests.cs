using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TodoApi.Controllers;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApiTest
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            var authController = new AuthController(userServiceMock.Object);

            var loginRequest = new LoginRequest
            {
                Username = "TestUser",
                Password = "TestPassword123"
            };

            var expectedUser = new User
            {
                Id = 1,
                Username = "TestUser",
                PasswordHash = "TestPassword123"
            };

            userServiceMock.Setup(x => x.RegisterUser(It.IsAny<LoginRequest>()))
                        .ReturnsAsync(expectedUser);

            // Act
            var result = await authController.Register(loginRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var user = Assert.IsType<User>(createdResult.Value);
            Assert.Equal("Register", createdResult.ActionName);
            Assert.Equal(expectedUser.Id, user.Id);
            Assert.Equal(expectedUser.Username, user.Username);
            Assert.Equal(expectedUser.PasswordHash, user.PasswordHash);
        }
        [Fact]
        public async Task Register_ValidRequest_ReturnsUsernameisExists()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            var authController = new AuthController(userServiceMock.Object);

            var loginRequest = new LoginRequest
            {
                Username = "TestUser",
                Password = "TestPassword123"
            };

            var expectedUser = new User
            {
                Id = 1,
                Username = "TestUser",
                PasswordHash = "TestPassword123"
            };
            userServiceMock.Setup(x => x.UserNameExists("TestUser")).Returns(true);

            // Act
            var result = await authController.Register(loginRequest);

            // Assert
            var exceptionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var exception = Assert.IsType<String>(exceptionResult.Value);
            Assert.Equal("Username is Exists", exception);
        }
        

        [Fact]
        public async Task Login_ValidRequest_ReturnsOk()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            var authController = new AuthController(userServiceMock.Object);
            var loginRequest = new LoginRequest
            {
                // Initialize login request properties
            };

            var userId = 1; // Simulate user ID
            var user = new User { Id = userId, Username = "TestUser" }; // Create a User object

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData, user.Id.ToString(), "userId")
            };

            var identity = new ClaimsIdentity(userClaims);
            var principal = new ClaimsPrincipal(identity);

            // Set the User property of the controller context to the mock user
            authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            userServiceMock.Setup(x => x.UsersFirstOrDefaultAsync(It.IsAny<LoginRequest>()))
                          .ReturnsAsync(user);
            userServiceMock.Setup(x => x.VerifyPasswordHash(loginRequest.Password, user.PasswordHash))
                          .Returns(true);

            userServiceMock.Setup(x => x.LoginUser(user))
                           .ReturnsAsync(new LoginResponse
                           {
                               // Initialize login response properties
                           });

            // Act
            var result = await authController.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            var authController = new AuthController(userServiceMock.Object);

            var loginRequest = new LoginRequest
            {
                // Initialize login request properties
            };
            var userId = 1; // Simulate user ID
            var user = new User { Id = userId, Username = "TestUser" }; // Create a User object

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData, user.Id.ToString(), "userId")
            };

            var identity = new ClaimsIdentity(userClaims);
            var principal = new ClaimsPrincipal(identity);

            // Set the User property of the controller context to the mock user
            authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            User userNull = null;
            userServiceMock.Setup(x => x.UsersFirstOrDefaultAsync(loginRequest)).ReturnsAsync(userNull);

            // Act
            var result = await authController.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsBadRequest()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            var authController = new AuthController(userServiceMock.Object);

            var loginRequest = new LoginRequest
            {
                // Initialize login request properties
            };
            var userId = 1; // Simulate user ID
            var user = new User { Id = userId, Username = "TestUser" }; // Create a User object

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData, user.Id.ToString(), "userId")
            };

            var identity = new ClaimsIdentity(userClaims);
            var principal = new ClaimsPrincipal(identity);

            // Set the User property of the controller context to the mock user
            authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            userServiceMock.Setup(x => x.UsersFirstOrDefaultAsync(It.IsAny<LoginRequest>()))
                          .ReturnsAsync(user);
            userServiceMock.Setup(x => x.VerifyPasswordHash(loginRequest.Password, user.PasswordHash))
                          .Returns(false);

            // Act
            var result = await authController.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Wrong password.", badRequestResult.Value);
        }
    }
}
