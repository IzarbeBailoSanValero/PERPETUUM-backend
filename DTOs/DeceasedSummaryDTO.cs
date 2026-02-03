namespace PERPETUUM.DTOs;

public class DeceasedSummaryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoURL { get; set; }
    public DateTime DeathDate { get; set; }
}