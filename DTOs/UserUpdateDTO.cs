using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;


//no hará falta pedir el id del usuario a editar porque se saca del token o de la url
public class UserUpdateDto// Permite editar datos, pero NO la contraseña ni rol
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
}