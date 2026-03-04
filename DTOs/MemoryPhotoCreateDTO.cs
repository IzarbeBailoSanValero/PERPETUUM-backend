using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PERPETUUM.DTOs;

public class MemoryPhotoCreateDTO
{
    [Required]
    public int DeceasedId { get; set; }

    // Se fuerza a "Photo" (3) por defecto; el front puede omitirlo.
    public int Type { get; set; } = 3;

    public string? TextContent { get; set; }

    public string? AuthorRelation { get; set; }

    [Required]
    public IFormFile Photo { get; set; } = default!;
}

