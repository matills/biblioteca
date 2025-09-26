using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Models
{
    public class Prestamo
    {
        [Key]
        public int PrestamoId { get; set; }
        
        [Required]
        [Display(Name = "Fecha de Préstamo")]
        [DataType(DataType.Date)]
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;
        
        [Required]
        [Display(Name = "Fecha de Devolución Esperada")]
        [DataType(DataType.Date)]
        public DateTime FechaDevolucionEsperada { get; set; }
        
        [Display(Name = "Fecha de Devolución Real")]
        [DataType(DataType.Date)]
        public DateTime? FechaDevolucionReal { get; set; }
        
        [Display(Name = "Estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "Activo"; // Activo, Devuelto, Vencido
        
        [StringLength(500)]
        public string Observaciones { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public int UsuarioId { get; set; }
        
        [Required]
        [Display(Name = "Libro")]
        public int LibroId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }
        
        [ForeignKey("LibroId")]
        public virtual Libro Libro { get; set; }
    }
}