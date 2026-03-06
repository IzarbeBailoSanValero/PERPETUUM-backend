namespace PERPETUUM.DTOs;

public class MemoryResponseDTO
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public string Type { get; set; } = string.Empty;   // 1 condolence, 2 photo, 3 anecdote, --> en el pero caso devuelvo string vacio
     public string Status { get; set; } = string.Empty; //  0 pending, 1 accepted,2 rejected 
    public string? TextContent { get; set; }
    public string? MediaURL { get; set; }
    public string? AuthorRelation { get; set; }
    /// <summary>Nombre del usuario que publicó el recuerdo.</summary>
    public string? AuthorName { get; set; }

    /// <summary>FK a User. NULL cuando el autor es un Guardian.</summary>
    public int? UserId { get; set; }

    /// <summary>FK a MemorialGuardian. Relleno solo cuando el autor es Guardian.</summary>
    public int? GuardianAuthorId { get; set; }

    public int DeceasedId { get; set; }
    /// <summary>Opcional; se rellena en listados de moderación (pendientes).</summary>
    public string? DeceasedName { get; set; }
}