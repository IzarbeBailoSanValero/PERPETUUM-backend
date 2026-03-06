namespace PERPETUUM.Models;


public class Memory
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public MemoryType Type { get; set; }
    public MemoryStatus Status { get; set; } = MemoryStatus.Pending;
    public string? TextContent { get; set; }
    public string? MediaURL { get; set; }
    public string? AuthorRelation { get; set; }
    public int DeceasedId { get; set; }

    /// <summary>FK a User. NULL cuando el autor es un Guardian.</summary>
    public int? UserId { get; set; }

    /// <summary>FK a MemorialGuardian. Relleno solo cuando el autor es Guardian.</summary>
    public int? GuardianAuthorId { get; set; }

    /// <summary>Nombre del usuario que publicó el recuerdo. Se rellena al hacer JOIN con User en consultas de listado.</summary>
    public string? AuthorName { get; set; }
}