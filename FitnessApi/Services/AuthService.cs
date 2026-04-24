using FitnessApi.DTOs;
using FitnessApi.Models;
using FitnessApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FitnessApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email already in use" };
            }

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = $"Registration failed: {errors}" };
            }

            // Optional: Assign a default role here
            // await _userManager.AddToRoleAsync(user, "GymGoer");

            return new AuthResponseDto { IsSuccess = true, Message = "User created successfully." };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials." };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials." };
            }

            var token = await _jwtTokenGenerator.GenerateJwtToken(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token
            };
        }
    }
}
