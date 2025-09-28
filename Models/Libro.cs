using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Models
{
    public class Libro
    {
        [Key]
        public int LibroId { get; set; }
        
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ISBN es obligatorio")]
        [StringLength(20)]
        [RegularExpression(@"^[0-9\-]*$", ErrorMessage = "El ISBN solo puede contener números y guiones")]
        public string ISBN { get; set; } = string.Empty;
        
        [Display(Name = "Año de Publicación")]
        [Range(1000, 2030, ErrorMessage = "Año debe estar entre 1000 y 2030")]
        public int AnoPublicacion { get; set; }
        
        [Display(Name = "Número de Páginas")]
        [Range(1, 10000, ErrorMessage = "Debe tener entre 1 y 10000 páginas")]
        public int NumeroPaginas { get; set; }
        
        [StringLength(500)]
        public string? Descripcion { get; set; }
        
        [Display(Name = "Imagen de Portada")]
        public string? ImagenPortada { get; set; }
        
        [Display(Name = "Cantidad Disponible")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        public int CantidadDisponible { get; set; }

        [Required]
        [Display(Name = "Autor")]
        public int AutorId { get; set; }
        
        [Required]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [ForeignKey("AutorId")]
        public virtual Autor? Autor { get; set; }
        
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }
        
        public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}