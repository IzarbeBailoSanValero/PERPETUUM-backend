using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

// Hereda de CreateDTO para tener las mismas validaciones 
public class DeceasedUpdateDTO : DeceasedCreateDTO
{
    [Required(ErrorMessage = "El ID del difunto es obligatorio para actualizar")]
    public int Id { get; set; }
}