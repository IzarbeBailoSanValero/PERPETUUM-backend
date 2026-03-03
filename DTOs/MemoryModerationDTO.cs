namespace PERPETUUM.DTOs;

public class MemoryModerationDTO
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int Type { get; set; }
    public int Status { get; set; }
    public string? TextContent { get; set; }
    public string? MediaURL { get; set; }
    public string? AuthorRelation { get; set; }
    public int UserId { get; set; }
    public int DeceasedId { get; set; }
    public string DeceasedName { get; set; } = string.Empty;
}
