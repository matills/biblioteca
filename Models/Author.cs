using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Autor
    {
        [Key]
        public int AutorId { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;
        
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
        
        [StringLength(500)]
        public string? Biografia { get; set; }

        [Display(Name = "País de Origen")]
        [StringLength(50)]
        public string? Pais { get; set; }

        public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
    }
}