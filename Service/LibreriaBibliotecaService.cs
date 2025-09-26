using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;
using System.Text;

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

        public async Task<int> GetPrestamosVencidosAsync()
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
            var prestamosVencidos = await GetPrestamosVencidosAsync();
            
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

        public async Task<List<Prestamo>> GetPrestamosVencidosAsync()
        {
            return await _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Libro)
                    .ThenInclude(l => l.Autor)
                .Where(p => p.Estado == "Activo" && p.FechaDevolucionEsperada < DateTime.Now)
                .OrderBy(p => p.FechaDevolucionEsperada)
                .ToListAsync();
        }
    }
}