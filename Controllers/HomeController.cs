using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Biblioteca.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILibreriaBibliotecaService _bibliotecaService;

        public HomeController(ILogger<HomeController> logger, ILibreriaBibliotecaService bibliotecaService)
        {
            _logger = logger;
            _bibliotecaService = bibliotecaService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var estadisticas = new
                {
                    TotalLibros = await _bibliotecaService.GetTotalLibrosAsync(),
                    TotalUsuarios = await _bibliotecaService.GetTotalUsuariosAsync(),
                    PrestamosActivos = await _bibliotecaService.GetPrestamosActivosAsync(),
                    PrestamosVencidos = await _bibliotecaService.GetPrestamosVencidosCountAsync(),
                    LibrosMasPrestados = await _bibliotecaService.GetLibrosMasPrestadosAsync(5),
                    UsuariosMasActivos = await _bibliotecaService.GetUsuariosMasActivosAsync(5),
                    PrestamosProximosVencer = await _bibliotecaService.GetPrestamosProximosVencerAsync(3)
                };

                ViewBag.TotalLibros = estadisticas.TotalLibros;
                ViewBag.TotalUsuarios = estadisticas.TotalUsuarios;
                ViewBag.PrestamosActivos = estadisticas.PrestamosActivos;
                ViewBag.PrestamosVencidos = estadisticas.PrestamosVencidos;
                ViewBag.LibrosMasPrestados = estadisticas.LibrosMasPrestados;
                ViewBag.UsuariosMasActivos = estadisticas.UsuariosMasActivos;
                ViewBag.PrestamosProximosVencer = estadisticas.PrestamosProximosVencer;

                _logger.LogInformation("Dashboard cargado exitosamente con {TotalLibros} libros y {TotalUsuarios} usuarios",
                    estadisticas.TotalLibros, estadisticas.TotalUsuarios);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
                TempData["Error"] = "Error al cargar las estadísticas del sistema";
                return View();
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> GenerarReporte()
        {
            try
            {
                var excelBytes = await _bibliotecaService.GenerarReporteExcelAsync();
                var fileName = $"Reporte_Biblioteca_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el reporte Excel");
                TempData["Error"] = "Error al generar el reporte Excel";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEstadisticasJson()
        {
            try
            {
                var estadisticas = new
                {
                    totalLibros = await _bibliotecaService.GetTotalLibrosAsync(),
                    totalUsuarios = await _bibliotecaService.GetTotalUsuariosAsync(),
                    prestamosActivos = await _bibliotecaService.GetPrestamosActivosAsync(),
                    prestamosVencidos = await _bibliotecaService.GetPrestamosVencidosCountAsync(),
                    fechaActualizacion = DateTime.Now
                };

                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas JSON");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}