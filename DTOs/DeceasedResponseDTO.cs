namespace PERPETUUM.DTOs;

public class DeceasedResponseDTO
{
    public int Id { get; set; } 
    public int FuneralHomeId { get; set; }
    public int GuardianId { get; set; }
    public int StaffId { get; set; }
    public string Dni { get; set; }
    public string Name { get; set; }
    public string Epitaph { get; set; }
    public string Biography { get; set; }
    public string PhotoURL { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime DeathDate { get; set; }

    public List<MemoryResponseDTO>? Memories { get; set; }
}