using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;


public class UserDtoIn
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Name { get; set; } = string.Empty; 

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email incorrecto")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}