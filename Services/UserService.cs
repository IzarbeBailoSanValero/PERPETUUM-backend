using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Repositories;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserDtoOut?> GetUserProfileAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserDtoOut
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            BirthDate = user.BirthDate,
            Role = Roles.StandardUser
        };
    }

    public async Task<bool> UpdateUserProfileAsync(int id, UserUpdateDto dto) //no modifica contraseñas, eso si acaso implemento en auth //coge el id de la url! por eso no lo pido al front en el put
    {
       

        var user = await _repository.GetByIdAsync(id);
        if (user == null) return false;


        user.Name = dto.Name;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.BirthDate = dto.BirthDate;

        _logger.LogInformation($"Actualizando perfil del usuario {id}");
        return await _repository.UpdateAsync(user);
    }

    public async Task<bool> DeleteUserAccountAsync(int id)
    {
        _logger.LogWarning($"Usuario {id} dándose de baja.");
        return await _repository.DeleteAsync(id);
    }

    
}