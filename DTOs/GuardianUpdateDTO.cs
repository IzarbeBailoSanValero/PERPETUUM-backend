using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class GuardianUpdateDTO
{
    [Required]
    public int Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(9)]
    public string? Dni { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }
}
