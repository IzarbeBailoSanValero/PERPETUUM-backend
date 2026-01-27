using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class StaffCreateDTO
{
    [Required(ErrorMessage = "El ID de la funeraria es obligatorio")]
    public int FuneralHomeId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(9, ErrorMessage = "El DNI no puede superar los 9 caracteres")]
    public string DNI { get; set; }


    [Required]
    [StringLength(20, MinimumLength = 6)]
    public string Password { get; set; } 

    public bool IsAdmin { get; set; } = false; 
}