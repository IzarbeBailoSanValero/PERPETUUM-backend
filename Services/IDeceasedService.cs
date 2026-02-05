using PERPETUUM.DTOs;

namespace PERPETUUM.Services;

public interface IDeceasedService
{
    Task<int> CreateDeceasedAsync(DeceasedCreateDTO createDTO);
    Task<List<DeceasedResponseDTO>> GetAllDeceasedAsync();
    Task<DeceasedResponseDTO?> GetDeceasedProfileAsync(int id);
    Task<List<DeceasedSummaryDTO>> GetByGuardianIdAsync(int guardianId);
   
    Task<bool> UpdateDeceasedAsync(DeceasedUpdateDTO updateDTO);
    Task<bool> DeleteDeceasedAsync(int id);
    Task<List<DeceasedResponseDTO>> SearchDeceasedAsync(DeceasedSearchDTO searchDTO);
}