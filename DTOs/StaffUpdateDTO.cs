using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class StaffUpdateDTO 
{
   [Required(ErrorMessage = "El ID del empleado es obligatorio")]
    public int Id { get; set; }

    // he ELIMINADO FuneralHomeId porque es mucho lío mover a un empleado de empresa, por eso no hereda del create

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [StringLength(255, ErrorMessage = "El email no puede superar los 255 caracteres")]
    public string Email { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(9, ErrorMessage = "El DNI no puede superar los 9 caracteres")]
    public string DNI { get; set; }
}