using BoticAPI.DTOs;
using BoticAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoticAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login with email and password to get JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var (success, message, token) = await _authService.LoginAsync(request.Email, request.Password);

            if (!success)
            {
                return Unauthorized(new { message });
            }

            return Ok(new { token, message = "Login successful" });
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Name, email, and password are required" });
            }

            var (success, message, userId) = await _authService.RegisterAsync(request.Name, request.Email, request.Password, request.RoleId);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { userId, message = "Registration successful" });
        }
    }
}