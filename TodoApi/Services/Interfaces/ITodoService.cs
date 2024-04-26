using static TodoApi.Models.Enums;
using TodoApi.Models;
using TodoApi.DTOs;

namespace TodoApi.Services.Interfaces
{
    public interface ITodoService
    {

        Task<List<Todo>> GetAllTodosAsync(int pageNumber, int pageSize,int userId);

        Task<Todo> GetTodoByIdAsync(int id,int userId);

        Task<bool> UpdateTodoAsync(TodoUpdateRequest todoRequest,int userId);

        Task<Todo> CreateTodoAsync(TodoRequest todoRequest,int userId);

        Task<bool> DeleteTodoAsync(int id,int userId);

        int Count(int userId);

    }
}
