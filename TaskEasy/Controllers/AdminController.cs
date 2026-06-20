using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEasy.Data;

namespace TaskEasy.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet("users")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.Plan,
                    u.PlanExpira,
                    CantidadProyectos = u.Projects.Count
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // DELETE
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Role == "Admin")
                return BadRequest("No se puede eliminar a un administrador.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET
        [HttpGet("payments")]
        public async Task<IActionResult> GetTodosPagos()
        {
            var pagos = await _context.Payments
                .Include(p => p.User)
                .Select(p => new
                {
                    p.Id,
                    Usuario = p.User!.Username,
                    p.Monto,
                    p.Metodo,
                    p.Estado,
                    p.Fecha
                })
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return Ok(pagos);
        }
    }
}