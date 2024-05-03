using Microsoft.EntityFrameworkCore;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;
using static TodoApi.Models.Enums;
using AutoMapper;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly IMapper _mapper;
        private readonly MasterDBContext _context;

        public TodoService(MasterDBContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Todo>> GetAllTodosAsync(int pageNumber, int pageSize,int userId)
        {
                return await _context.Todos.Where(t => t.UserId == userId && !t.IsDeleted)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        }

        public async Task<Todo> GetTodoByIdAsync(int id,int userId)
        {
            return await _context.Todos.Where(t=>t.UserId == userId).Where(d=>d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateTodoAsync(TodoUpdateRequest todoRequest,int userId)
        {
            var todo = await _context.Todos.Where(t => t.UserId == userId && !t.IsDeleted).Where(d=>d.Id == todoRequest.Id).FirstOrDefaultAsync();
            if (todo == null)
            {
                return false;
            }
            todo.Description = todoRequest.Description;
            todo.Title = todoRequest.Title;
            todo.Status = todoRequest.Status;
            todo.DueDate = todoRequest.DueDate;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<Todo> CreateTodoAsync(TodoRequest todoRequest,int userId)
        {
            Todo todo = _mapper.Map<Todo>(todoRequest);
            todo.UserId = userId;
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            return todo;
        }

        public async Task<bool> DeleteTodoAsync(int id,int userId)
        {
            var todo = await _context.Todos.Where(t => t.UserId == userId).Where(d => d.Id == id).FirstOrDefaultAsync();
            if (todo == null)
            {
                return false;
            }

            todo.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public  int Count(int userId)
        {
            var total =  _context.Todos.Where(t=>t.UserId == userId && !t.IsDeleted).Count();
            return total;  
        }

    }
}
