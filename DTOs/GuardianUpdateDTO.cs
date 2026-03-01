using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class GuardianUpdateDTO
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(9)]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    
    public int FuneralHomeId { get; set; }
    public int StaffId { get; set; }
}