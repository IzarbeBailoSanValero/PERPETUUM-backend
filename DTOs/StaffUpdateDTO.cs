using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class StaffUpdateDTO : StaffCreateDTO
{
    [Required(ErrorMessage = "El ID del empleado es obligatorio")]
    public int Id { get; set; }
}