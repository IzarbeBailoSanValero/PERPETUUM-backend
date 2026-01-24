namespace PERPETUUM.DTOs;

public class MemoryResponseDTO
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // antes eran numeros 0,1,2,3... peor los transformamos aquí a strings descriptivos
    public string Type { get; set; }   // 1 condolence, 2 photo, 3 anecdote,    
     public string Status { get; set; } //  0 pending, 1 accepted,2 rejected 
    public string? TextContent { get; set; }
    public string? MediaURL { get; set; }
    public string? AuthorRelation { get; set; }
    
    public int UserId { get; set; }
    public int DeceasedId { get; set; }
}

