using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using ResumeManagement.Application.DTOs;
using ResumeManagement.Application.Interfaces;
using ResumeManagement.Models;
using ResumeManagement.Persistence;
using System;

namespace ResumeManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ResumeDbContext _context;
        private readonly ITokenService _tokenService;

        public UserService(ResumeDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        public async Task RegisterAsync(RegisterDto dto)
        {
            var user = await _context.Users
                .Where(u => u.Email == dto.Email || u.Username == dto.Username)
                .FirstOrDefaultAsync();

            if (user != null)  
                throw new Exception("Email or Username already exists");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }


        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email
            ==dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            return _tokenService.GenerateToken(user);
        }
    }
}
