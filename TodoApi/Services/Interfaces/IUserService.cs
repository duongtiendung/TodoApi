using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUser(LoginRequest request);
        Task<LoginResponse> LoginUser(User user);
        bool UserNameExists(string username);
        bool VerifyPasswordHash(string password, string passwordHash);

        Task<User> UsersFirstOrDefaultAsync(LoginRequest request);
    }
}
