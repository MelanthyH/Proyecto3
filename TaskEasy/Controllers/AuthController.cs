using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEasy.Data;
using TaskEasy.Models;
using TaskEasy.Models.DTOs;
using TaskEasy.Services;

namespace TaskEasy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, TokenService tokenService, IConfiguration config)
        {
            _context = context;
            _tokenService = tokenService;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            bool existe = await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email);
            if (existe)
                return BadRequest("El usuario o correo ya está registrado.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User",
                Plan = "Free"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado correctamente." });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

            var token = _tokenService.GenerateToken(user, _config);

            return Ok(new
            {
                token,
                usuario = new { user.Id, user.Username, user.Role, user.Plan }
            });
        }
    }
}