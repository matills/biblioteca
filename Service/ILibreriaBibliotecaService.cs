using Biblioteca.Models;

namespace Biblioteca.Services
{
    public interface ILibreriaBibliotecaService
    {
        Task<int> GetTotalLibrosAsync();
        Task<int> GetTotalUsuariosAsync();
        Task<int> GetPrestamosActivosAsync();
        Task<int> GetPrestamosVencidosAsync();
        
        Task<bool> VerificarDisponibilidadLibroAsync(int libroId);
        Task<List<Libro>> GetLibrosMasPrestadosAsync(int cantidad = 10);
        Task<List<Usuario>> GetUsuariosMasActivosAsync(int cantidad = 10);
        
        Task<bool> PuedeRealizarPrestamoAsync(int usuarioId);
        Task<string> GenerarReporteEstadisticasAsync();
        
        Task<List<Prestamo>> GetPrestamosProximosVencerAsync(int dias = 3);
        Task<List<Prestamo>> GetPrestamosVencidosAsync();
    }
}