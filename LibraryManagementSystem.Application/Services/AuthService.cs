using LibraryManagementSystem.Core.Entities;
using LibraryManagementSystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace LibraryManagementSystem.Application.Services
{
    public class AuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public AuthService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow;
            _config = config;
        }

        public async Task<ApplicationUser> RegisterAsync(string username, string password, string email, string role)
        {
            var user = new ApplicationUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                Email = email,
                Creation = DateTime.UtcNow
            };
            await _uow.Users.AddAsync(user);
            await _uow.CompleteAsync();
            return user;
        }

        public string GenerateToken(ApplicationUser user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
