using BlogApp.Models;
using BlogApp.Models.BlogApp.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApp.Services
{
    public interface IUserService
    {
        Task<string> Register(User user);
        Task<string> Login(string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly BlogContext _context;
        private readonly IConfiguration _config;

        public UserService(BlogContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
                throw new InvalidOperationException("Email already exists");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return GenerateJwtToken(user);
        }

        public async Task<string> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
