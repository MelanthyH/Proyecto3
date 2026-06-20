using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEasy.Data;
using TaskEasy.Models;
using TaskEasy.Models.DTOs;

namespace TaskEasy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int LIMITE_PROYECTOS_FREE = 2;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET
        [HttpGet]
        public async Task<IActionResult> GetMisProyectos()
        {
            var userId = GetUserId();
            var proyectos = await _context.Projects
                .Where(p => p.UserId == userId)
                .Select(p => new { p.Id, p.Nombre, p.Descripcion, p.FechaCreacion })
                .ToListAsync();

            return Ok(proyectos);
        }

        // GET
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProyecto(int id)
        {
            var userId = GetUserId();
            var proyecto = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (proyecto == null) return NotFound();
            return Ok(proyecto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearProyecto([FromBody] ProjectDTO dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user!.Plan == "Free")
            {
                int cantidad = await _context.Projects.CountAsync(p => p.UserId == userId);
                if (cantidad >= LIMITE_PROYECTOS_FREE)
                    return BadRequest($"Límite de {LIMITE_PROYECTOS_FREE} proyectos alcanzado en el plan Free. Actualiza a Pro para crear más.");
            }

            var proyecto = new Project
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                UserId = userId
            };

            _context.Projects.Add(proyecto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProyecto), new { id = proyecto.Id }, proyecto);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarProyecto(int id, [FromBody] ProjectDTO dto)
        {
            var userId = GetUserId();
            var proyecto = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (proyecto == null) return NotFound();

            proyecto.Nombre = dto.Nombre;
            proyecto.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();
            return Ok(proyecto);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProyecto(int id)
        {
            var userId = GetUserId();
            var proyecto = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (proyecto == null) return NotFound();

            _context.Projects.Remove(proyecto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}