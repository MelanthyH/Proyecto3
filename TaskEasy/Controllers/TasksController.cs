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
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int LIMITE_TAREAS_FREE = 5;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<Project?> GetProyectoPropio(int projectId, int userId) =>
            await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        // GET
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTareasDeProyecto(int projectId)
        {
            var userId = GetUserId();
            var proyecto = await GetProyectoPropio(projectId, userId);
            if (proyecto == null) return NotFound("Proyecto no encontrado.");

            var tareas = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            return Ok(tareas);
        }

        // GET
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTarea(int id)
        {
            var userId = GetUserId();
            var tarea = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project!.UserId == userId);

            if (tarea == null) return NotFound();
            return Ok(tarea);
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> CrearTarea([FromBody] TaskDTO dto)
        {
            var userId = GetUserId();
            var proyecto = await GetProyectoPropio(dto.ProjectId, userId);
            if (proyecto == null) return BadRequest("Proyecto no válido.");

            var user = await _context.Users.FindAsync(userId);
            if (user!.Plan == "Free")
            {
                int cantidad = await _context.Tasks.CountAsync(t => t.ProjectId == dto.ProjectId);
                if (cantidad >= LIMITE_TAREAS_FREE)
                    return BadRequest($"Límite de {LIMITE_TAREAS_FREE} tareas por proyecto en el plan Free. Actualiza a Pro para más.");
            }

            var tarea = new TaskItem
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado,
                FechaLimite = dto.FechaLimite,
                ProjectId = dto.ProjectId
            };

            _context.Tasks.Add(tarea);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTarea), new { id = tarea.Id }, tarea);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TaskDTO dto)
        {
            var userId = GetUserId();
            var tarea = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project!.UserId == userId);

            if (tarea == null) return NotFound();

            tarea.Titulo = dto.Titulo;
            tarea.Descripcion = dto.Descripcion;
            tarea.Estado = dto.Estado;
            tarea.FechaLimite = dto.FechaLimite;

            await _context.SaveChangesAsync();
            return Ok(tarea);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarTarea(int id)
        {
            var userId = GetUserId();
            var tarea = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project!.UserId == userId);

            if (tarea == null) return NotFound();

            _context.Tasks.Remove(tarea);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}