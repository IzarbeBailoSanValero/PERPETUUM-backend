using PERPETUUM.DTOs;
using PERPETUUM.Models;

namespace PERPETUUM.Services;

public interface IMemoryService
{
    // Obtener memorias --> parametro para filtrar si solo aprobadas o todas
    Task<List<MemoryResponseDTO>> GetByDeceasedIdAsync(int deceasedId, bool onlyApproved);
    Task<MemoryResponseDTO?> GetByIdAsync(int id);
    
    Task<int> AddMemoryAsync(MemoryCreateDTO dto, int? userId, int? guardianAuthorId);
    
    Task<bool> UpdateStatusAsync(int id, MemoryStatus status); //aprobar y rechazar memorias

    /// <summary>Lista memorias pendientes; si deceasedIds es null, todas (Admin); si no, solo de esos difuntos (Guardian).</summary>
    Task<List<MemoryResponseDTO>> GetPendingMemoriesAsync(List<int>? deceasedIds);

    Task<bool> DeleteMemoryAsync(int id); //soft delete
}