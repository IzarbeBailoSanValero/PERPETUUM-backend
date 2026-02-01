using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class GuardianCreateDTO
{
    [Required(ErrorMessage = "El campo FuneralHomeId es obligatorio.")]
    public int FuneralHomeId { get; set; }

    [Required(ErrorMessage = "El campo StaffId es obligatorio.")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
     [StringLength(9, ErrorMessage = "El nombre no puede superar los 9 caracteres.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El número de teléfono no es válido.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
