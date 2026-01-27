using PERPETUUM.Models;

namespace PERPETUUM.Repositories;
public interface IMemoryRepository
{
    // Obtiene TODAS las memorias --> para panel de admin
    Task<List<Memory>> GetByDeceasedIdAsync(int deceasedId);

    // Obtiene  APROBADAS --> para el obituario
    Task<List<Memory>> GetApprovedByDeceasedIdAsync(int deceasedId);

    // CRUD: añadir, borrar, get by id
    Task<int> AddAsync(Memory memory);
    Task<bool> DeleteAsync(int id);
    Task<Memory?> GetByIdAsync(int id);
    
    // Aceptar/rechazar memoria --> para el memorialGuardian
    Task<bool> UpdateStatusAsync(int id, MemoryStatus status);
    

    
}