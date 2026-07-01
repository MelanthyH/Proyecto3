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
        private readonly IWebHostEnvironment _env;

        public AuthController(AppDbContext context, TokenService tokenService, IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _tokenService = tokenService;
            _config = config;
            _env = env;
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

        // Esta ruta solo funciona cuando el entorno es Development para evitar riesgos en producción.
        [AllowAnonymous]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDTO dto)
        {
            if (!_env.IsDevelopment())
                return Forbid();

            bool existe = await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email);
            if (existe)
                return BadRequest("El usuario o correo ya está registrado.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Admin",
                Plan = "Pro"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario administrador creado correctamente (solo Development)." });
        }
    }
}