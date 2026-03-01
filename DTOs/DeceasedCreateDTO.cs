using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class DeceasedCreateDTO
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    public string Dni { get; set; }


    [Required]
    public int FuneralHomeId { get; set; }

    [Required]
    public int GuardianId { get; set; }

    [Required]
    public int StaffId { get; set; }


    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; }


    [Required(ErrorMessage = "El epitafio es obligatorio")]
    [StringLength(255)]
    public string Epitaph { get; set; }


    [Required(ErrorMessage = "La biografía es obligatoria")]
    public string Biography { get; set; }


    [Required(ErrorMessage = "La URL de la foto es obligatoria")]
    [StringLength(500, ErrorMessage = "La URL es demasiado larga")]
    [Url(ErrorMessage = "El formato de la URL no es válido")]
    public string PhotoURL { get; set; }


    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DeathDate { get; set; }
}