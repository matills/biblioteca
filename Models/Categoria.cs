using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }
        
        [StringLength(200)]
        public string? Descripcion { get; set; }

        public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
    }
}