using PERPETUUM.DTOs;

namespace PERPETUUM.Services;

public interface IFuneralHomeService
{
    Task<List<FuneralHomeResponseDTO>> GetAllAsync();
    Task<FuneralHomeResponseDTO?> GetByIdAsync(int id);
    Task<int> CreateAsync(FuneralHomeCreateDTO dto);
    Task<bool> UpdateAsync(FuneralHomeUpdateDTO dto);
    Task<bool> DeleteAsync(int id);
}