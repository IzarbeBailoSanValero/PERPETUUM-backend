using PERPETUUM.DTOs;
using PERPETUUM.Models;

namespace PERPETUUM.Services;

public interface IMemoryService
{
    // Obtener memorias --> parametro para filtrar si solo aprobadas o todas
    Task<List<MemoryResponseDTO>> GetByDeceasedIdAsync(int deceasedId, bool onlyApproved);
    
    Task<int> AddMemoryAsync(MemoryCreateDTO dto);
    
    Task<bool> UpdateStatusAsync(int id, MemoryStatus status); //aprobar y rechazar memorias
    
    Task<bool> DeleteMemoryAsync(int id); //soft delete
}