using Auth.Models;
using Auth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<object> RegisterAsync(RegisterModel model)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
            {
                return new { Success = false, Message = "User already exists." };
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Rating = 0
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new { Success = true, Message = "User registered successfully." };
        }

        public async Task<object> LoginAsync(LoginModel model)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return new { Success = false, Message = "Invalid username or password." };
            }

            var token = GenerateJwtToken(user);
            return new { Success = true, Token = token };
        }

        public async Task<object> GetUserProfileAsync(int userId)
        {
            var user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Rating
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new { Success = false, Message = "User not found." };
            }

            return new { Success = true, User = user };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}