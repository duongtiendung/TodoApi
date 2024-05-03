using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Controllers;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;
using static TodoApi.Models.Enums;

namespace TodoApiTest
{
    public class TodoesControllerTests
    {
        private readonly Mock<ITodoService> _todoServiceMock;
        private readonly TodosController _todosController;

        public TodoesControllerTests()
        {
            _todoServiceMock = new Mock<ITodoService>();
            _todosController = new TodosController(_todoServiceMock.Object);
        }
        [Fact]
        public async Task GetTodos_ReturnsOk()
        {
            // Arrange
            var todosList = new List<Todo>
            {
                new Todo { Id = 1, Title = "Todo 1", Description = "Description 1", Status = Status.New, DueDate = DateTime.Now.AddDays(1),UserId=1 },
                new Todo { Id = 2, Title = "Todo 2", Description = "Description 2", Status = Status.Rejected, DueDate = DateTime.Now.AddDays(2) },
                new Todo { Id = 3, Title = "Todo 3", Description = "Description 3", Status = Status.Pending, DueDate = DateTime.Now.AddDays(3) }
            };

            _todoServiceMock.Setup(x => x.GetAllTodosAsync(It.IsAny <int>(), It.IsAny<int>(), It.IsAny<int>()))
                           .ReturnsAsync(todosList);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User") 
            }));

            // Set the User property of the controller context to the mock user
            _todosController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = await _todosController.GetTodos(It.IsAny<int>(), It.IsAny<int>());
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var anonymousType = okResult.Value.GetType();
            var todosProperty = anonymousType.GetProperty("Todos");
            var todos = Assert.IsAssignableFrom<List<Todo>>(todosProperty.GetValue(okResult.Value));
            Assert.Equal(todosList, todos);
        }

        [Fact]
        public async Task GetTodo_ReturnsOkResult_WhenTodoExists()
        {
            // Arrange
            int todoId = 1;
            int userId = 123;
            _todoServiceMock.Setup(service => service.GetTodoByIdAsync(todoId, userId))
                .ReturnsAsync(new Todo { Id = todoId, UserId = userId, Title = "Sample Todo" });
            var controller = new TodosController(_todoServiceMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.UserData, userId.ToString())
            }, "mock"));

            // Act
            var result = await controller.GetTodo(todoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var todo = Assert.IsType<Todo>(okResult.Value);
            Assert.Equal(todoId, todo.Id);
            Assert.Equal(userId, todo.UserId);
            Assert.Equal("Sample Todo", todo.Title);
        }

        [Fact]
        public async Task GetTodo_ReturnsNotFound_WhenTodoDoesNotExist()
        {
            // Arrange
            int todoId = 1;
            int userId = 123;
            _todoServiceMock.Setup(service => service.GetTodoByIdAsync(todoId, userId))
                .ReturnsAsync((Todo)null);
            var controller = new TodosController(_todoServiceMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.UserData, userId.ToString())
            }, "mock"));

            // Act
            var result = await controller.GetTodo(todoId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutTodo_WithValidRequest_ReturnsNoContent()
        {
            // Arrange
            var todoRequest = new TodoUpdateRequest { Id = 1, Title = "Updated Todo", Description = "Updated Description" };
            var userId = 1; // Simulate user ID

            _todoServiceMock.Setup(x => x.UpdateTodoAsync(todoRequest, userId)).ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData,userId.ToString(),"userId")
            }));

            // Set the User property of the controller context to the mock user
            _todosController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = await _todosController.PutTodo(todoRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutTodo_WithInvalidRequest_ReturnsNotFound()
        {
            // Arrange
            var todoRequest = new TodoUpdateRequest { Id = 1, Title = "Updated Todo", Description = "Updated Description" };
            var userId = 1; // Simulate user ID

            _todoServiceMock.Setup(x => x.UpdateTodoAsync(todoRequest, userId)).ReturnsAsync(false);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData,userId.ToString(),"userId")
            }));

            // Set the User property of the controller context to the mock user
            _todosController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = await _todosController.PutTodo(todoRequest);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostTodo_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var todoRequest = new TodoRequest { Title = "New Todo", Description = "New Description" };
            var userId = 1; // Simulate user ID
            var createdTodo = new Todo { Id = 1, Title = "New Todo", Description = "New Description" };

            _todoServiceMock.Setup(x => x.CreateTodoAsync(todoRequest, userId)).ReturnsAsync(createdTodo);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData,userId.ToString(),"userId")
            }));

            // Set the User property of the controller context to the mock user
            _todosController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = await _todosController.PostTodo(todoRequest);

            // Assert
            ActionResult<Todo> actionResult = result;
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(TodosController.GetTodo), createdAtActionResult.ActionName);
            Assert.Equal(createdTodo.Id, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(createdTodo, createdAtActionResult.Value);
        }

        [Fact]
        public async Task DeleteTodo_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var todoId = 1;
            var userId = 1; // Simulate user ID

            _todoServiceMock.Setup(x => x.DeleteTodoAsync(todoId, userId)).ReturnsAsync(true);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData,userId.ToString(),"userId")
            }));

            // Set the User property of the controller context to the mock user
            _todosController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = await _todosController.DeleteTodo(todoId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTodo_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var todoId = 1;
            var userId = 1; // Simulate user ID

            _todoServiceMock.Setup(x => x.DeleteTodoAsync(todoId, userId)).ReturnsAsync(false);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.UserData,userId.ToString(),"userId")
            }));

            // Set the User property of the controller context to the mock user
            _todosController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = await _todosController.DeleteTodo(todoId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
