using PERPETUUM.Models;

namespace PERPETUUM.Repositories;

public interface IMemorialGuardianRepository
{
    Task<MemorialGuardian?> GetByEmailAsync(string email);
    Task<MemorialGuardian?> GetByIdAsync(int id);
    Task<int> AddAsync(MemorialGuardian guardian);
    Task<List<MemorialGuardian>> GetAllAsync();
    Task<bool> UpdateAsync(MemorialGuardian guardian);
    Task<bool> DeleteAsync(int id);
}