
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Repositories;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace PERPETUUM.Services;


public class MemorialGuardianService : IMemorialGuardianService
{
    private readonly IMemorialGuardianRepository _repository;
    private readonly ILogger<MemorialGuardianService> _logger;

    public MemorialGuardianService(IMemorialGuardianRepository repository, ILogger<MemorialGuardianService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> CreateGuardianAsync(GuardianCreateDTO dto)
    {
        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null) throw new ArgumentException("El email ya está registrado.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var guardian = new MemorialGuardian
        {
            FuneralHomeId = dto.FuneralHomeId,
            StaffId = dto.StaffId,
            Name = dto.Name,
            Dni = dto.Dni,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = passwordHash
        };

        return await _repository.AddAsync(guardian);
    }





    public async Task<GuardianResponseDTO?> GetGuardianByIdAsync(int id)
    {
        var guardian = await _repository.GetByIdAsync(id);
        if (guardian == null) return null;

        return new GuardianResponseDTO
        {
            Id = guardian.Id,
            Name = guardian.Name,
            Email = guardian.Email,
            PhoneNumber = guardian.PhoneNumber,
            FuneralHomeId = guardian.FuneralHomeId
        };
    }
}