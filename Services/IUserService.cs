using PERPETUUM.DTOs;

namespace PERPETUUM.Services;

public interface IUserService
{
    Task<UserDtoOut?> GetUserProfileAsync(int id);
    Task<bool> UpdateUserProfileAsync(int id, userUpdateDto dto);
    Task<bool> DeleteUserAccountAsync(int id);
}