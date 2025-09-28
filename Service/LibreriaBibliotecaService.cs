using Biblioteca.Data;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using OfficeOpenXml;

namespace Biblioteca.Services
{
    public class LibreriaBibliotecaService : ILibreriaBibliotecaService
    {
        private readonly BibliotecaContext _context;

        public LibreriaBibliotecaService(BibliotecaContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalLibrosAsync()
        {
            return await _context.Libros.CountAsync();
        }

        public async Task<int> GetTotalUsuariosAsync()
        {
            return await _context.Usuarios.CountAsync();
        }

        public async Task<int> GetPrestamosActivosAsync()
        {
            return await _context.Prestamos.CountAsync(p => p.Estado == "Activo");
        }

        public async Task<int> GetPrestamosVencidosCountAsync()
        {
            return await _context.Prestamos.CountAsync(p =>
                p.Estado == "Vencido" ||
                (p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now));
        }

        public async Task<bool> VerificarDisponibilidadLibroAsync(int libroId)
        {
            var libro = await _context.Libros.FindAsync(libroId);
            if (libro == null) return false;

            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.LibroId == libroId && p.Estado == "Activo");

            return libro.CantidadDisponible > prestamosActivos;
        }

        public async Task<List<Libro>> GetLibrosMasPrestadosAsync(int cantidad = 10)
        {
            return await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .Select(l => new {
                    Libro = l,
                    TotalPrestamos = l.Prestamos.Count
                })
                .OrderByDescending(x => x.TotalPrestamos)
                .Take(cantidad)
                .Select(x => x.Libro)
                .ToListAsync();
        }

        public async Task<List<Usuario>> GetUsuariosMasActivosAsync(int cantidad = 10)
        {
            return await _context.Usuarios
                .Select(u => new {
                    Usuario = u,
                    TotalPrestamos = u.Prestamos.Count
                })
                .OrderByDescending(x => x.TotalPrestamos)
                .Take(cantidad)
                .Select(x => x.Usuario)
                .ToListAsync();
        }

        public async Task<bool> PuedeRealizarPrestamoAsync(int usuarioId)
        {
            var tieneVencidos = await _context.Prestamos
                .AnyAsync(p => p.UsuarioId == usuarioId &&
                          (p.Estado == "Vencido" ||
                           (p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now)));

            if (tieneVencidos) return false;

            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.UsuarioId == usuarioId && p.Estado == "Activo");

            return prestamosActivos < 3;
        }

        public async Task<string> GenerarReporteEstadisticasAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== REPORTE ESTADÍSTICAS BIBLIOTECA ===");
            sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine();

            var totalLibros = await GetTotalLibrosAsync();
            var totalUsuarios = await GetTotalUsuariosAsync();
            var prestamosActivos = await GetPrestamosActivosAsync();
            var prestamosVencidos = await GetPrestamosVencidosCountAsync();

            sb.AppendLine("ESTADÍSTICAS GENERALES:");
            sb.AppendLine($"- Total de Libros: {totalLibros}");
            sb.AppendLine($"- Total de Usuarios: {totalUsuarios}");
            sb.AppendLine($"- Préstamos Activos: {prestamosActivos}");
            sb.AppendLine($"- Préstamos Vencidos: {prestamosVencidos}");
            sb.AppendLine();

            var librosMasPrestados = await GetLibrosMasPrestadosAsync(5);
            sb.AppendLine("LIBROS MÁS PRESTADOS:");
            for (int i = 0; i < librosMasPrestados.Count; i++)
            {
                var libro = librosMasPrestados[i];
                var totalPrestamos = await _context.Prestamos.CountAsync(p => p.LibroId == libro.LibroId);
                sb.AppendLine($"{i + 1}. {libro.Titulo} - {libro.Autor?.Nombre} ({totalPrestamos} préstamos)");
            }
            sb.AppendLine();

            var usuariosMasActivos = await GetUsuariosMasActivosAsync(5);
            sb.AppendLine("USUARIOS MÁS ACTIVOS:");
            for (int i = 0; i < usuariosMasActivos.Count; i++)
            {
                var usuario = usuariosMasActivos[i];
                var totalPrestamos = await _context.Prestamos.CountAsync(p => p.UsuarioId == usuario.UsuarioId);
                sb.AppendLine($"{i + 1}. {usuario.Nombre} ({totalPrestamos} préstamos)");
            }

            return sb.ToString();
        }

        public async Task<List<Prestamo>> GetPrestamosProximosVencerAsync(int dias = 3)
        {
            var fechaLimite = DateTime.Now.AddDays(dias);

            return await _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Libro)
                    .ThenInclude(l => l.Autor)
                .Where(p => p.Estado == "Activo" &&
                           p.FechaDevolucionEsperada <= fechaLimite &&
                           p.FechaDevolucionEsperada >= DateTime.Now)
                .OrderBy(p => p.FechaDevolucionEsperada)
                .ToListAsync();
        }

        public async Task<List<Prestamo>> GetPrestamosVencidosListAsync()
        {
            return await _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Libro)
                    .ThenInclude(l => l.Autor)
                .Where(p => p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now)
                .OrderBy(p => p.FechaDevolucionEsperada)
                .ToListAsync();
        }

        public async Task<byte[]> GenerarReporteExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            
            var wsEstadisticas = package.Workbook.Worksheets.Add("Estadísticas Generales");
            
            var totalLibros = await GetTotalLibrosAsync();
            var totalUsuarios = await GetTotalUsuariosAsync();
            var prestamosActivos = await GetPrestamosActivosAsync();
            var prestamosVencidos = await GetPrestamosVencidosCountAsync();

            wsEstadisticas.Cells[1, 1].Value = "REPORTE ESTADÍSTICAS BIBLIOTECA";
            wsEstadisticas.Cells[1, 1].Style.Font.Bold = true;
            wsEstadisticas.Cells[1, 1].Style.Font.Size = 16;
            
            wsEstadisticas.Cells[2, 1].Value = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";
            
            wsEstadisticas.Cells[4, 1].Value = "Estadística";
            wsEstadisticas.Cells[4, 2].Value = "Valor";
            wsEstadisticas.Cells[4, 1, 4, 2].Style.Font.Bold = true;
            
            wsEstadisticas.Cells[5, 1].Value = "Total de Libros";
            wsEstadisticas.Cells[5, 2].Value = totalLibros;
            wsEstadisticas.Cells[6, 1].Value = "Total de Usuarios";
            wsEstadisticas.Cells[6, 2].Value = totalUsuarios;
            wsEstadisticas.Cells[7, 1].Value = "Préstamos Activos";
            wsEstadisticas.Cells[7, 2].Value = prestamosActivos;
            wsEstadisticas.Cells[8, 1].Value = "Préstamos Vencidos";
            wsEstadisticas.Cells[8, 2].Value = prestamosVencidos;

            var wsLibros = package.Workbook.Worksheets.Add("Libros Más Prestados");
            var librosMasPrestados = await GetLibrosMasPrestadosAsync(10);
            
            wsLibros.Cells[1, 1].Value = "Posición";
            wsLibros.Cells[1, 2].Value = "Título";
            wsLibros.Cells[1, 3].Value = "Autor";
            wsLibros.Cells[1, 4].Value = "Categoría";
            wsLibros.Cells[1, 5].Value = "Total Préstamos";
            wsLibros.Cells[1, 1, 1, 5].Style.Font.Bold = true;

            for (int i = 0; i < librosMasPrestados.Count; i++)
            {
                var libro = librosMasPrestados[i];
                var totalPrestamos = await _context.Prestamos.CountAsync(p => p.LibroId == libro.LibroId);
                
                wsLibros.Cells[i + 2, 1].Value = i + 1;
                wsLibros.Cells[i + 2, 2].Value = libro.Titulo;
                wsLibros.Cells[i + 2, 3].Value = libro.Autor?.Nombre ?? "Sin autor";
                wsLibros.Cells[i + 2, 4].Value = libro.Categoria?.Nombre ?? "Sin categoría";
                wsLibros.Cells[i + 2, 5].Value = totalPrestamos;
            }

            var wsUsuarios = package.Workbook.Worksheets.Add("Usuarios Más Activos");
            var usuariosMasActivos = await GetUsuariosMasActivosAsync(10);
            
            wsUsuarios.Cells[1, 1].Value = "Posición";
            wsUsuarios.Cells[1, 2].Value = "Nombre";
            wsUsuarios.Cells[1, 3].Value = "Email";
            wsUsuarios.Cells[1, 4].Value = "Total Préstamos";
            wsUsuarios.Cells[1, 1, 1, 4].Style.Font.Bold = true;

            for (int i = 0; i < usuariosMasActivos.Count; i++)
            {
                var usuario = usuariosMasActivos[i];
                var totalPrestamos = await _context.Prestamos.CountAsync(p => p.UsuarioId == usuario.UsuarioId);
                
                wsUsuarios.Cells[i + 2, 1].Value = i + 1;
                wsUsuarios.Cells[i + 2, 2].Value = usuario.Nombre;
                wsUsuarios.Cells[i + 2, 3].Value = usuario.Email;
                wsUsuarios.Cells[i + 2, 4].Value = totalPrestamos;
            }

            var wsPrestamosVencidos = package.Workbook.Worksheets.Add("Préstamos Vencidos");
            var prestamosVencidosList = await GetPrestamosVencidosListAsync();
            
            wsPrestamosVencidos.Cells[1, 1].Value = "Usuario";
            wsPrestamosVencidos.Cells[1, 2].Value = "Libro";
            wsPrestamosVencidos.Cells[1, 3].Value = "Autor";
            wsPrestamosVencidos.Cells[1, 4].Value = "Fecha Préstamo";
            wsPrestamosVencidos.Cells[1, 5].Value = "Fecha Vencimiento";
            wsPrestamosVencidos.Cells[1, 6].Value = "Días Vencido";
            wsPrestamosVencidos.Cells[1, 1, 1, 6].Style.Font.Bold = true;

            for (int i = 0; i < prestamosVencidosList.Count; i++)
            {
                var prestamo = prestamosVencidosList[i];
                var diasVencido = (DateTime.Now - prestamo.FechaDevolucionEsperada).Days;
                
                wsPrestamosVencidos.Cells[i + 2, 1].Value = prestamo.Usuario?.Nombre ?? "Sin usuario";
                wsPrestamosVencidos.Cells[i + 2, 2].Value = prestamo.Libro?.Titulo ?? "Sin libro";
                wsPrestamosVencidos.Cells[i + 2, 3].Value = prestamo.Libro?.Autor?.Nombre ?? "Sin autor";
                wsPrestamosVencidos.Cells[i + 2, 4].Value = prestamo.FechaPrestamo.ToString("dd/MM/yyyy");
                wsPrestamosVencidos.Cells[i + 2, 5].Value = prestamo.FechaDevolucionEsperada.ToString("dd/MM/yyyy");
                wsPrestamosVencidos.Cells[i + 2, 6].Value = diasVencido;
            }

            foreach (var worksheet in package.Workbook.Worksheets)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }
    }
}