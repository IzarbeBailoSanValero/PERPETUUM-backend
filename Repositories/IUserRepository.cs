using PERPETUUM.DTOs;
using PERPETUUM.Models;

namespace PERPETUUM.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<int> CreateUserAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    //todo: implementar crud

}