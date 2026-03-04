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
    
    public int UserId { get; set; }
    public int DeceasedId { get; set; }
    /// <summary>Opcional; se rellena en listados de moderación (pendientes).</summary>
    public string? DeceasedName { get; set; }
}

