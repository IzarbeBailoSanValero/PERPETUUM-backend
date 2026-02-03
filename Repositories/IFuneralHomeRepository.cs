using PERPETUUM.Models;

namespace PERPETUUM.Repositories;

public interface IFuneralHomeRepository
{
    Task<List<FuneralHome>> GetAllAsync();
    Task<FuneralHome?> GetByIdAsync(int? id);
    Task<int> AddAsync(FuneralHome funeralHome);
    Task<bool> UpdateAsync(FuneralHome funeralHome);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByCifAsync(string cif, int? excludeId = null);

}