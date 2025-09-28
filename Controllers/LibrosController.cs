using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;
using OfficeOpenXml;

namespace Biblioteca.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LibrosController(BibliotecaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var libros = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .ToListAsync();
            
            ViewBag.Categorias = await _context.Categorias.Select(c => c.Nombre).Distinct().ToListAsync();
            return View(libros);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .Include(l => l.Prestamos)
                    .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.LibroId == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["AutorId"] = new SelectList(await _context.Autores.ToListAsync(), "AutorId", "Nombre");
            ViewData["CategoriaId"] = new SelectList(await _context.Categorias.ToListAsync(), "CategoriaId", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LibroId,Titulo,ISBN,AnoPublicacion,NumeroPaginas,Descripcion,CantidadDisponible,AutorId,CategoriaId")] Libro libro, IFormFile? imagenFile)
        {
            try
            {
                // Verificar si el ISBN ya existe
                if (!string.IsNullOrEmpty(libro.ISBN) && await _context.Libros.AnyAsync(l => l.ISBN == libro.ISBN))
                {
                    ModelState.AddModelError("ISBN", "Ya existe un libro con este ISBN");
                }

                if (ModelState.IsValid)
                {
                    // Procesar imagen si se subió una
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        try
                        {
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "libros");
                            Directory.CreateDirectory(uploadsFolder);
                            
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagenFile.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imagenFile.CopyToAsync(fileStream);
                            }
                            
                            libro.ImagenPortada = "/images/libros/" + uniqueFileName;
                        }
                        catch (Exception ex)
                        {
                            TempData["Warning"] = $"El libro se guardó pero hubo un error con la imagen: {ex.Message}";
                        }
                    }

                    _context.Add(libro);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Libro creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el libro: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error en Create Libro: {ex}");
            }

            ViewData["AutorId"] = new SelectList(await _context.Autores.ToListAsync(), "AutorId", "Nombre", libro.AutorId);
            ViewData["CategoriaId"] = new SelectList(await _context.Categorias.ToListAsync(), "CategoriaId", "Nombre", libro.CategoriaId);
            return View(libro);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }

            ViewData["AutorId"] = new SelectList(await _context.Autores.ToListAsync(), "AutorId", "Nombre", libro.AutorId);
            ViewData["CategoriaId"] = new SelectList(await _context.Categorias.ToListAsync(), "CategoriaId", "Nombre", libro.CategoriaId);
            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LibroId,Titulo,ISBN,AnoPublicacion,NumeroPaginas,Descripcion,ImagenPortada,CantidadDisponible,AutorId,CategoriaId")] Libro libro, IFormFile? imagenFile)
        {
            if (id != libro.LibroId)
            {
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(libro.ISBN) && await _context.Libros.AnyAsync(l => l.ISBN == libro.ISBN && l.LibroId != id))
                {
                    ModelState.AddModelError("ISBN", "Ya existe otro libro con este ISBN");
                }

                if (ModelState.IsValid)
                {
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(libro.ImagenPortada))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, libro.ImagenPortada.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "libros");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagenFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagenFile.CopyToAsync(fileStream);
                        }
                        
                        libro.ImagenPortada = "/images/libros/" + uniqueFileName;
                    }

                    _context.Update(libro);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Libro actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LibroExists(libro.LibroId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el libro: {ex.Message}";
            }

            ViewData["AutorId"] = new SelectList(await _context.Autores.ToListAsync(), "AutorId", "Nombre", libro.AutorId);
            ViewData["CategoriaId"] = new SelectList(await _context.Categorias.ToListAsync(), "CategoriaId", "Nombre", libro.CategoriaId);
            return View(libro);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .Include(l => l.Prestamos)
                .FirstOrDefaultAsync(m => m.LibroId == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var libro = await _context.Libros
                    .Include(l => l.Prestamos)
                    .FirstOrDefaultAsync(l => l.LibroId == id);

                if (libro != null)
                {
                    var prestamosActivos = libro.Prestamos.Where(p => p.Estado == "Activo").ToList();
                    if (prestamosActivos.Any())
                    {
                        TempData["Error"] = $"No se puede eliminar el libro porque tiene {prestamosActivos.Count} préstamos activos";
                        return RedirectToAction(nameof(Index));
                    }

                    if (!string.IsNullOrEmpty(libro.ImagenPortada))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, libro.ImagenPortada.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    _context.Libros.Remove(libro);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Libro eliminado exitosamente";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar el libro: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ImportarExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Por favor seleccione un archivo Excel";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        var librosImportados = 0;
                        var errores = new List<string>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var titulo = worksheet.Cells[row, 1].Text?.Trim();
                                var isbn = worksheet.Cells[row, 2].Text?.Trim();
                                var autorNombre = worksheet.Cells[row, 3].Text?.Trim();
                                var categoriaNombre = worksheet.Cells[row, 4].Text?.Trim();
                                
                                if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(isbn))
                                {
                                    errores.Add($"Fila {row}: Título e ISBN son obligatorios");
                                    continue;
                                }

                                var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Nombre == autorNombre);
                                if (autor == null)
                                {
                                    autor = new Autor { Nombre = autorNombre ?? "Autor Desconocido" };
                                    _context.Autores.Add(autor);
                                    await _context.SaveChangesAsync();
                                }

                                var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == categoriaNombre);
                                if (categoria == null)
                                {
                                    categoria = new Categoria { Nombre = categoriaNombre ?? "Sin Categoría" };
                                    _context.Categorias.Add(categoria);
                                    await _context.SaveChangesAsync();
                                }

                                if (await _context.Libros.AnyAsync(l => l.ISBN == isbn))
                                {
                                    errores.Add($"Fila {row}: Ya existe un libro con ISBN {isbn}");
                                    continue;
                                }

                                var libro = new Libro
                                {
                                    Titulo = titulo,
                                    ISBN = isbn,
                                    AutorId = autor.AutorId,
                                    CategoriaId = categoria.CategoriaId,
                                    AnoPublicacion = int.TryParse(worksheet.Cells[row, 5].Text, out int ano) ? ano : DateTime.Now.Year,
                                    NumeroPaginas = int.TryParse(worksheet.Cells[row, 6].Text, out int paginas) ? paginas : 1,
                                    CantidadDisponible = int.TryParse(worksheet.Cells[row, 7].Text, out int cantidad) ? cantidad : 1,
                                    Descripcion = worksheet.Cells[row, 8].Text?.Trim()
                                };

                                _context.Libros.Add(libro);
                                librosImportados++;
                            }
                            catch (Exception ex)
                            {
                                errores.Add($"Fila {row}: {ex.Message}");
                            }
                        }

                        await _context.SaveChangesAsync();

                        if (errores.Any())
                        {
                            TempData["Warning"] = $"Se importaron {librosImportados} libros con {errores.Count} errores: {string.Join(", ", errores.Take(3))}";
                        }
                        else
                        {
                            TempData["Success"] = $"Se importaron {librosImportados} libros exitosamente";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar el archivo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LibroExists(int id)
        {
            return _context.Libros.Any(e => e.LibroId == id);
        }

        public ActionResult BuscarLibrosPorCategoria(int categoriaId)
        {
            var libros = _context.Libros
                .Where(l => l.CategoriaId == categoriaId)
                .Select(l => new { l.LibroId, l.Titulo, l.CantidadDisponible })
                .ToList();
            
            return Json(libros);
        }

        public async Task<IActionResult> VerificarDisponibilidad(int libroId)
        {
            var libro = await _context.Libros.FindAsync(libroId);
            if (libro == null)
                return NotFound();

            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.LibroId == libroId && p.Estado == "Activo");

            var disponible = libro.CantidadDisponible > prestamosActivos;

            return Json(new { disponible, cantidadDisponible = libro.CantidadDisponible - prestamosActivos });
        }
    }
}
