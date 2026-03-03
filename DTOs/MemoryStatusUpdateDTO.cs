using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class MemoryStatusUpdateDTO
{
    [Range(1, 2, ErrorMessage = "El estado solo puede ser 1 (Approved) o 2 (Rejected).")]
    public int Status { get; set; }
}
