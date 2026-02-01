using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class MemoryCreateDTO
{
    [Required]
    public int DeceasedId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int Type { get; set; }  // 1: Condolence, 2: Anecdote, 3: Photo

    public string? TextContent { get; set; }

    public string? MediaURL { get; set; }

    public string? AuthorRelation { get; set; }
}