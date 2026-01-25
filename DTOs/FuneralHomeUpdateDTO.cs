using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class FuneralHomeUpdateDTO : FuneralHomeCreateDTO
{
    [Required(ErrorMessage = "El ID es obligatorio para actualizar")]
    public int Id { get; set; }
}