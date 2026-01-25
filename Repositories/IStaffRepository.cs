using PERPETUUM.Models;

namespace PERPETUUM.Repositories;

public interface IStaffRepository
{
   
    Task<List<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<int> AddAsync(Staff staff);
    Task<bool> UpdateAsync(Staff staff);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<bool> ExistsByDniAsync(string dni, int? excludeId = null);
    Task<List<Staff>> GetByFuneralHomeIdAsync(int funeralHomeId);
}