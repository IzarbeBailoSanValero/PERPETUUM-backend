namespace PERPETUUM.Models;


public class Memory
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public MemoryType Type { get; set; }
    public MemoryStatus Status { get; set; } = MemoryStatus.Pending;
    public string? TextContent { get; set; } //can be null
    public string? MediaURL { get; set; }//can be null
    public string? AuthorRelation { get; set; }//can be null
    public int DeceasedId { get; set; }
    public int UserId { get; set; }
}