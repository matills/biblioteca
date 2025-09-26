using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;

namespace Biblioteca.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly BibliotecaContext _context;

        public UsuariosController(BibliotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Prestamos)
                    .ThenInclude(p => p.Libro)
                .ToListAsync();
            
            return View(usuarios);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Prestamos)
                    .ThenInclude(p => p.Libro)
                        .ThenInclude(l => l.Autor)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsuarioId,Nombre,Email,Telefono,Direccion,FechaRegistro")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario registrado con este email");
                    return View(usuario);
                }

                usuario.FechaRegistro = DateTime.Now;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UsuarioId,Nombre,Email,Telefono,Direccion,FechaRegistro")] Usuario usuario)
        {
            if (id != usuario.UsuarioId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email && u.UsuarioId != id))
                    {
                        ModelState.AddModelError("Email", "Ya existe otro usuario registrado con este email");
                        return View(usuario);
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Usuario actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.UsuarioId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Prestamos)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Prestamos)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario != null)
            {
                var prestamosActivos = usuario.Prestamos.Where(p => p.Estado == "Activo").ToList();
                if (prestamosActivos.Any())
                {
                    TempData["Error"] = $"No se puede eliminar el usuario porque tiene {prestamosActivos.Count} préstamos activos";
                    return RedirectToAction(nameof(Index));
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reporte()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Prestamos)
                    .ThenInclude(p => p.Libro)
                .ToListAsync();

            var reporte = usuarios.Select(u => new
            {
                Usuario = u,
                TotalPrestamos = u.Prestamos.Count,
                PrestamosActivos = u.Prestamos.Count(p => p.Estado == "Activo"),
                PrestamosVencidos = u.Prestamos.Count(p => p.Estado == "Vencido"),
                UltimoPrestamo = u.Prestamos.OrderByDescending(p => p.FechaPrestamo).FirstOrDefault()?.FechaPrestamo
            }).OrderByDescending(r => r.TotalPrestamos).ToList();

            return View(reporte);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }

        public ActionResult GetUsuariosActivos()
        {
            var usuariosActivos = _context.Usuarios
                .Where(u => u.Prestamos.Any(p => p.Estado == "Activo"))
                .Select(u => new { 
                    u.UsuarioId, 
                    u.Nombre, 
                    u.Email,
                    PrestamosActivos = u.Prestamos.Count(p => p.Estado == "Activo")
                })
                .ToList();
            
            return Json(usuariosActivos);
        }

        public async Task<IActionResult> VerificarEmail(string email, int? usuarioId = null)
        {
            var existeEmail = await _context.Usuarios
                .AnyAsync(u => u.Email == email && (usuarioId == null || u.UsuarioId != usuarioId));

            return Json(!existeEmail);
        }
    }
}