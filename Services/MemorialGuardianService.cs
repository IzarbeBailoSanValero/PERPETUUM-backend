
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Repositories;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace PERPETUUM.Services;


public class MemorialGuardianService : IMemorialGuardianService
{
    private readonly IMemorialGuardianRepository _repository;
    private readonly IDeceasedRepository _deceasedRepository;
    private readonly ILogger<MemorialGuardianService> _logger;

    public MemorialGuardianService(IMemorialGuardianRepository repository, ILogger<MemorialGuardianService> logger, IDeceasedRepository deceasedRepository)
    {
        _repository = repository;
        _logger = logger;
        _deceasedRepository = deceasedRepository;
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

        var response = new GuardianResponseDTO
        {
            Id = guardian.Id,
            Name = guardian.Name,
            Email = guardian.Email,
            PhoneNumber = guardian.PhoneNumber,
            FuneralHomeId = guardian.FuneralHomeId
        };

        //añado lista de memoriales que gestionan
        response.DeceasedList = await _deceasedRepository.GetByGuardianIdAsync(guardian.Id);

        return response;
    }

    public async Task<List<GuardianResponseDTO>> GetAllGuardiansAsync()
    {
        var guardians = await _repository.GetAllAsync();
        var response = new List<GuardianResponseDTO>();

        foreach (var guardian in guardians)
        {
            var dto = new GuardianResponseDTO
            {
                Id = guardian.Id,
                Name = guardian.Name,
                Email = guardian.Email,
                PhoneNumber = guardian.PhoneNumber,
                FuneralHomeId = guardian.FuneralHomeId,
                DeceasedList = await _deceasedRepository.GetByGuardianIdAsync(guardian.Id)
            };
            response.Add(dto);
        }

        return response;
    }

 public async Task<bool> UpdateGuardianAsync(GuardianUpdateDTO dto)
{
    var existing = await _repository.GetByIdAsync(dto.Id);
    if (existing == null) return false;

    // Solo actualizar campos que tienen valor no vacío
    if (!string.IsNullOrWhiteSpace(dto.Name)) existing.Name = dto.Name;
    if (!string.IsNullOrWhiteSpace(dto.Dni)) existing.Dni = dto.Dni;
    
    if (!string.IsNullOrWhiteSpace(dto.Email) && existing.Email != dto.Email)
    {
        var emailCheck = await _repository.GetByEmailAsync(dto.Email);
        if (emailCheck != null && emailCheck.Id != dto.Id)
        {
            throw new ArgumentException("El email ya está registrado por otro guardián.");
        }
        existing.Email = dto.Email;
    }
    
    if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) existing.PhoneNumber = dto.PhoneNumber;

    return await _repository.UpdateAsync(existing);
}

    public async Task<bool> DeleteGuardianAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        return await _repository.DeleteAsync(id);
    }
}