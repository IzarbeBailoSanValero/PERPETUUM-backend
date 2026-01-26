using PERPETUUM.DTOs;
using PERPETUUM.Models;

namespace PERPETUUM.Repositories;

public interface IUserRepository
{
    // ... tus métodos CRUD anteriores (GetById, etc.) ...

    // MÉTODOS PARA AUTH
    UserDtoOut? GetUserFromCredentials(LoginDtoIn loginDto); // Devuelve null si falla
    UserDtoOut AddUserFromCredentials(UserDtoIn userDto);
}