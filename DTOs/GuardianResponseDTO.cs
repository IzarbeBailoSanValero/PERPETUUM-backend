namespace PERPETUUM.DTOs;

public class GuardianResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int FuneralHomeId { get; set; }
    public List<DeceasedSummaryDTO> DeceasedList { get; set; } = new List<DeceasedSummaryDTO>(); //evito inicializar en constructor, se crea directamente
}
