using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services
{
    public class UserService : IUserService
    {
        private readonly MasterDBContext _context;
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration, MasterDBContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<User> RegisterUser(LoginRequest request)
        {
            string passwordHash = CreatePasswordHash(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash =passwordHash,
                
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<LoginResponse> LoginUser(User user)
        {
            string token = CreateToken(user);
            LoginResponse response = new LoginResponse
            {
                Token = token
            };
            return response;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.UserData, user.Id.ToString(),"Id"),
            new Claim(ClaimTypes.Role, "User")
        };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPasswordHash(string password, string passwordHash)
        {
          return   BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<User?> UsersFirstOrDefaultAsync(LoginRequest request)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        }
        

        public bool UserNameExists(string username)
        {
            return (_context.Users?.Any(e => e.Username == username)).GetValueOrDefault(); 
        }
    }
}
