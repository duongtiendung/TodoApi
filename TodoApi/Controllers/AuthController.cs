using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(LoginRequest request)
        {
            if (_userService.UserNameExists(request.Username))
            {
                return BadRequest("Username is Exists");
            }
            var user = await _userService.RegisterUser(request);
            return CreatedAtAction("Register", new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> Login(LoginRequest request)
        {
            var user = await _userService.UsersFirstOrDefaultAsync(request);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!_userService.VerifyPasswordHash(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }
            var response = await _userService.LoginUser(user);
            return Ok(response);
        }
    }
}
