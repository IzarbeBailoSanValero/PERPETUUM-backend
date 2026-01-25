using PERPETUUM.DTOs;

namespace PERPETUUM.Services;

public interface IStaffService
{
   
    Task<StaffResponseDTO?> GetByIdAsync(int id);
    Task<List<StaffResponseDTO>> GetByFuneralHomeIdAsync(int funeralHomeId);
    Task<int> CreateAsync(StaffCreateDTO dto);
    Task<bool> UpdateAsync(StaffUpdateDTO dto);
    Task<bool> DeleteAsync(int id);
}