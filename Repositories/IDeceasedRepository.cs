using PERPETUUM.Models;
using PERPETUUM.DTOs;

namespace PERPETUUM.Repositories;

public interface IDeceasedRepository
{
    Task <List<Deceased>> GetAllAsync();
    Task <Deceased?> GetByIdAsync(int id);
    Task <int> AddAsync (Deceased deceased);
    Task <bool> UpdateAsync(Deceased deceased);
    Task<bool> DeleteAsync(int id);
    Task <List<Memory>> GetMemoriesByDeceasedIdAsync (int deceased);
    Task <List<Deceased>> SearchAsync(DeceasedSearchDTO searchDTO);
    Task<bool> ExistsByDniAsync(string dni, int? excludeId = null);

}