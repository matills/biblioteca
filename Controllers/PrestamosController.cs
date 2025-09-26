using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;

namespace Biblioteca.Controllers
{
    public class PrestamosController : Controller
    {
        private readonly BibliotecaContext _context;

        public PrestamosController(BibliotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string filtro = "todos")
        {
            var prestamosQuery = _context.Prestamos
                .Include(p => p.Libro)
                    .ThenInclude(l => l.Autor)
                .Include(p => p.Usuario)
                .AsQueryable();

            switch (filtro.ToLower())
            {
                case "activos":
                    prestamosQuery = prestamosQuery.Where(p => p.Estado == "Activo");
                    break;
                case "vencidos":
                    prestamosQuery = prestamosQuery.Where(p => p.Estado == "Vencido" || 
                        (p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now));
                    break;
                case "devueltos":
                    prestamosQuery = prestamosQuery.Where(p => p.Estado == "Devuelto");
                    break;
            }

            var prestamos = await prestamosQuery
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();

            foreach (var prestamo in prestamos.Where(p => p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now))
            {
                prestamo.Estado = "Vencido";
                _context.Update(prestamo);
            }
            await _context.SaveChangesAsync();

            ViewBag.FiltroActual = filtro;
            ViewBag.TotalPrestamos = await _context.Prestamos.CountAsync();
            ViewBag.PrestamosActivos = await _context.Prestamos.CountAsync(p => p.Estado == "Activo");
            ViewBag.PrestamosVencidos = await _context.Prestamos.CountAsync(p => p.Estado == "Vencido");
            ViewBag.PrestamosDevueltos = await _context.Prestamos.CountAsync(p => p.Estado == "Devuelto");

            return View(prestamos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                    .ThenInclude(l => l.Autor)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.PrestamoId == id);

            if (prestamo == null)
            {
                return NotFound();
            }

            return View(prestamo);
        }

        public async Task<IActionResult> Create()
        {
            var librosDisponibles = await _context.Libros
                .Include(l => l.Autor)
                .Where(l => l.CantidadDisponible > 0)
                .Select(l => new {
                    LibroId = l.LibroId,
                    Display = $"{l.Titulo} - {l.Autor.Nombre} (Disponibles: {l.CantidadDisponible})"
                })
                .ToListAsync();

            ViewData["LibroId"] = new SelectList(librosDisponibles, "LibroId", "Display");
            ViewData["UsuarioId"] = new SelectList(await _context.Usuarios.ToListAsync(), "UsuarioId", "Nombre");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrestamoId,FechaPrestamo,FechaDevolucionEsperada,Observaciones,UsuarioId,LibroId")] Prestamo prestamo)
        {
            if (ModelState.IsValid)
            {
                var libro = await _context.Libros.FindAsync(prestamo.LibroId);
                if (libro == null)
                {
                    ModelState.AddModelError("LibroId", "El libro seleccionado no existe");
                    await CargarDatosCreate();
                    return View(prestamo);
                }

                var prestamosActivosLibro = await _context.Prestamos
                    .CountAsync(p => p.LibroId == prestamo.LibroId && p.Estado == "Activo");

                if (prestamosActivosLibro >= libro.CantidadDisponible)
                {
                    ModelState.AddModelError("LibroId", "No hay copias disponibles de este libro");
                    await CargarDatosCreate();
                    return View(prestamo);
                }

                var tieneVencidos = await _context.Prestamos
                    .AnyAsync(p => p.UsuarioId == prestamo.UsuarioId && p.Estado == "Vencido");

                if (tieneVencidos)
                {
                    ModelState.AddModelError("UsuarioId", "El usuario tiene préstamos vencidos. Debe devolverlos antes de realizar nuevos préstamos");
                    await CargarDatosCreate();
                    return View(prestamo);
                }

                prestamo.FechaPrestamo = DateTime.Now;
                prestamo.Estado = "Activo";

                if (prestamo.FechaDevolucionEsperada == default)
                {
                    prestamo.FechaDevolucionEsperada = DateTime.Now.AddDays(15);
                }

                _context.Add(prestamo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Préstamo registrado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            await CargarDatosCreate();
            return View(prestamo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null)
            {
                return NotFound();
            }

            ViewData["LibroId"] = new SelectList(await _context.Libros.Include(l => l.Autor).ToListAsync(), "LibroId", "Titulo", prestamo.LibroId);
            ViewData["UsuarioId"] = new SelectList(await _context.Usuarios.ToListAsync(), "UsuarioId", "Nombre", prestamo.UsuarioId);
            
            return View(prestamo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrestamoId,FechaPrestamo,FechaDevolucionEsperada,FechaDevolucionReal,Estado,Observaciones,UsuarioId,LibroId")] Prestamo prestamo)
        {
            if (id != prestamo.PrestamoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prestamo);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Préstamo actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrestamoExists(prestamo.PrestamoId))
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

            ViewData["LibroId"] = new SelectList(await _context.Libros.Include(l => l.Autor).ToListAsync(), "LibroId", "Titulo", prestamo.LibroId);
            ViewData["UsuarioId"] = new SelectList(await _context.Usuarios.ToListAsync(), "UsuarioId", "Nombre", prestamo.UsuarioId);
            
            return View(prestamo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Devolver(int id, string? observaciones)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null)
            {
                return NotFound();
            }

            if (prestamo.Estado != "Activo" && prestamo.Estado != "Vencido")
            {
                TempData["Error"] = "Solo se pueden devolver préstamos activos o vencidos";
                return RedirectToAction(nameof(Details), new { id });
            }

            prestamo.FechaDevolucionReal = DateTime.Now;
            prestamo.Estado = "Devuelto";
            
            if (!string.IsNullOrEmpty(observaciones))
            {
                prestamo.Observaciones += $" | Devolución: {observaciones}";
            }

            _context.Update(prestamo);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Libro devuelto exitosamente";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                    .ThenInclude(l => l.Autor)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.PrestamoId == id);

            if (prestamo == null)
            {
                return NotFound();
            }

            return View(prestamo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo != null)
            {
                if (prestamo.Estado == "Activo")
                {
                    TempData["Error"] = "No se puede eliminar un préstamo activo. Primero debe ser devuelto.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Prestamos.Remove(prestamo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Préstamo eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDatosCreate()
        {
            var librosDisponibles = await _context.Libros
                .Include(l => l.Autor)
                .Where(l => l.CantidadDisponible > 0)
                .Select(l => new {
                    LibroId = l.LibroId,
                    Display = $"{l.Titulo} - {l.Autor.Nombre} (Disponibles: {l.CantidadDisponible})"
                })
                .ToListAsync();

            ViewData["LibroId"] = new SelectList(librosDisponibles, "LibroId", "Display");
            ViewData["UsuarioId"] = new SelectList(await _context.Usuarios.ToListAsync(), "UsuarioId", "Nombre");
        }

        private bool PrestamoExists(int id)
        {
            return _context.Prestamos.Any(e => e.PrestamoId == id);
        }

        public ActionResult GetPrestamosVencidos()
        {
            var vencidos = _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Libro)
                .Where(p => p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now)
                .Select(p => new {
                    p.PrestamoId,
                    Usuario = p.Usuario.Nombre,
                    Libro = p.Libro.Titulo,
                    FechaDevolucionEsperada = p.FechaDevolucionEsperada,
                    DiasVencido = (DateTime.Now - p.FechaDevolucionEsperada).Days
                })
                .ToList();

            return Json(vencidos);
        }

        public async Task<IActionResult> VerificarDisponibilidadLibro(int libroId)
        {
            var libro = await _context.Libros.FindAsync(libroId);
            if (libro == null)
                return NotFound();

            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.LibroId == libroId && p.Estado == "Activo");

            var disponible = libro.CantidadDisponible > prestamosActivos;
            var cantidadDisponible = libro.CantidadDisponible - prestamosActivos;

            return Json(new { 
                disponible, 
                cantidadDisponible,
                totalStock = libro.CantidadDisponible,
                prestamosActivos
            });
        }

        [HttpPost]
        public async Task<IActionResult> RenovarPrestamo(int id, int diasExtension = 15)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null)
                return NotFound();

            if (prestamo.Estado != "Activo")
            {
                return Json(new { success = false, message = "Solo se pueden renovar préstamos activos" });
            }

            var tieneVencidos = await _context.Prestamos
                .AnyAsync(p => p.UsuarioId == prestamo.UsuarioId && 
                          p.PrestamoId != id && 
                          p.Estado == "Vencido");

            if (tieneVencidos)
            {
                return Json(new { success = false, message = "El usuario tiene otros préstamos vencidos" });
            }

            prestamo.FechaDevolucionEsperada = prestamo.FechaDevolucionEsperada.AddDays(diasExtension);
            prestamo.Observaciones += $" | Renovado el {DateTime.Now:dd/MM/yyyy} por {diasExtension} días";

            _context.Update(prestamo);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Préstamo renovado exitosamente",
                nuevaFechaDevolucion = prestamo.FechaDevolucionEsperada.ToString("dd/MM/yyyy")
            });
        }
    }
}