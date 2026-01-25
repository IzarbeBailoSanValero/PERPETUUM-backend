using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class FuneralHomeCreateDTO
{
    [Required(ErrorMessage = "El nombre de la funeraria es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "El CIF es obligatorio")]
    [StringLength(9, ErrorMessage = "El CIF no puede superar los 9 caracteres")]
    public string CIF { get; set; }

    [Required(ErrorMessage = "El email de contacto es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string ContactEmail { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(255)]
    public string Address { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string PhoneNumber { get; set; }
}