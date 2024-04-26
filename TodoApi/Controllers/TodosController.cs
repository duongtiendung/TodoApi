using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;
using static TodoApi.Models.Enums;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }


        // GET: api/Todos
        [HttpGet, Authorize]
        public async Task<IActionResult> GetTodos(int pageNumber, int pageSize)
        {
            var userId = Convert.ToInt32( User.FindFirstValue(ClaimTypes.UserData)) ;
            var totalCount = _todoService.Count(userId) ;
            var todos = await _todoService.GetAllTodosAsync( pageNumber,  pageSize,userId);
            var result = new
            {
                TotalCount = totalCount,
                Todos = todos
            };
            return Ok(result);
        }

        // GET: api/Todos/5
        [HttpGet("{id}"), Authorize]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var userId = Convert.ToInt32( User.FindFirstValue(ClaimTypes.UserData)) ;
            var todo = await _todoService.GetTodoByIdAsync(id,userId);
            if (todo == null)
            {
                return NotFound();
            }

            return Ok(todo);
        }

        // PUT: api/Todos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut , Authorize]
        public async Task<IActionResult> PutTodo(TodoUpdateRequest todoRequest)
        {
            var userId = Convert.ToInt32( User.FindFirstValue(ClaimTypes.UserData)) ;
            var success = await _todoService.UpdateTodoAsync( todoRequest,userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Todos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize]
        public async Task<ActionResult<Todo>> PostTodo(TodoRequest todoRequest)
        {
            var userId = Convert.ToInt32( User.FindFirstValue(ClaimTypes.UserData)) ;
            var createdTodo = await _todoService.CreateTodoAsync(todoRequest,userId);

            return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
        }

        // DELETE: Todoes/5s
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = Convert.ToInt32( User.FindFirstValue(ClaimTypes.UserData)) ;
            var success = await _todoService.DeleteTodoAsync(id,userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
