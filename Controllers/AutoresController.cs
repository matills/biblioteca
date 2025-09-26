using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;

namespace Biblioteca.Controllers
{
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;

        public AutoresController(BibliotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Autores.Include(a => a.Libros).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autor = await _context.Autores
                .Include(a => a.Libros)
                .FirstOrDefaultAsync(m => m.AutorId == id);
            
            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AutorId,Nombre,FechaNacimiento,Biografia,Pais")] Autor autor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(autor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Autor creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(autor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autor = await _context.Autores.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AutorId,Nombre,FechaNacimiento,Biografia,Pais")] Autor autor)
        {
            if (id != autor.AutorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(autor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Autor actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AutorExists(autor.AutorId))
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
            return View(autor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autor = await _context.Autores
                .Include(a => a.Libros)
                .FirstOrDefaultAsync(m => m.AutorId == id);
            
            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var autor = await _context.Autores.Include(a => a.Libros).FirstOrDefaultAsync(a => a.AutorId == id);
            
            if (autor != null)
            {
                if (autor.Libros.Any())
                {
                    TempData["Error"] = "No se puede eliminar el autor porque tiene libros asociados";
                    return RedirectToAction(nameof(Index));
                }
                
                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Autor eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AutorExists(int id)
        {
            return _context.Autores.Any(e => e.AutorId == id);
        }

        public ActionResult GetAutoresJson()
        {
            var autores = _context.Autores.Select(a => new { a.AutorId, a.Nombre }).ToList();
            return Json(autores);
        }

        public async Task<IActionResult> GetAutorAsync(int id)
        {
            var autor = await _context.Autores.FindAsync(id);
            if (autor == null)
                return NotFound();
            
            return Json(autor);
        }
    }
}