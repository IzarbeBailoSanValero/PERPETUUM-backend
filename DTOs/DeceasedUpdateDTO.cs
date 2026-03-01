using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class DeceasedUpdateDTO
{
    [Required(ErrorMessage = "El ID del difunto es obligatorio para actualizar")]
    public int Id { get; set; }
    
    public string? Dni { get; set; }
    public int? FuneralHomeId { get; set; }
    public int? GuardianId { get; set; }
    public int? StaffId { get; set; }
    public string? Name { get; set; }
    public string? Epitaph { get; set; }
    public string? Biography { get; set; }
    public string? PhotoURL { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
}
