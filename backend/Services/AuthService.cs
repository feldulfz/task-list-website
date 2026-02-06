using task_manager_api.DTOs;
using task_manager_api.Interfaces;
using TaskManager.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using task_manager_api.Extensions;
using TaskManager.Models;

namespace task_manager_api.Services
{
    public class AuthService(ApplicationDbContext context, ITokenService tokenService) : IAuthService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await EmailExistsAsync(registerDto.Email))
            {
                throw new InvalidOperationException("Email is already taken");
            }

            using var hmac = new HMACSHA512();

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.ToDto(_tokenService);
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }
            }            

            return user.ToDto(_tokenService);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }
    }
}