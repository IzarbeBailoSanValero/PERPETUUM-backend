using PERPETUUM.DTOs;

namespace PERPETUUM.Services;
public interface IMemorialGuardianService
{
    Task<int> CreateGuardianAsync(GuardianCreateDTO dto);
    Task<GuardianResponseDTO?> GetGuardianByIdAsync(int id);
    Task<List<GuardianResponseDTO>> GetAllGuardiansAsync();
    Task<bool> UpdateGuardianAsync(GuardianUpdateDTO dto);
    Task<bool> DeleteGuardianAsync(int id);
}