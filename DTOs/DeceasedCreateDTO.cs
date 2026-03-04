using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PERPETUUM.DTOs;

public class DeceasedCreateDTO
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public int FuneralHomeId { get; set; }

    [Required]
    public int GuardianId { get; set; }

    [Required]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Epitaph { get; set; }

    [Required(ErrorMessage = "La biografía es obligatoria")]
    public string Biography { get; set; } = string.Empty;

    /// <summary>URL de la foto. Obligatoria si no se envía Photo (creación por JSON). Si se envía Photo (multipart), se rellena tras subir a Cloudinary.</summary>
    [StringLength(500, ErrorMessage = "La URL es demasiado larga")]
    [Url(ErrorMessage = "El formato de la URL no es válido")]
    public string? PhotoURL { get; set; }

    /// <summary>Foto del difunto para subir a Cloudinary. Solo se usa en POST con multipart/form-data.</summary>
    public IFormFile? Photo { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DeathDate { get; set; }
}