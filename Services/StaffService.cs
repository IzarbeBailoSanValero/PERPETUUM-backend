using PERPETUUM.Repositories;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Services;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IFuneralHomeRepository _funeralHomeRepository;
    private readonly ILogger<StaffService> _logger;

    public StaffService(IStaffRepository staffRepository, IFuneralHomeRepository funeralHomeRepository, ILogger<StaffService> logger)
    {
        _staffRepository = staffRepository;
        _funeralHomeRepository = funeralHomeRepository;
        _logger = logger;
    }


    //no hago get al porque creo que no tiene sentido de negocio. en su lugar, hago get by funeral home, sí que es útil


    public async Task<StaffResponseDTO?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        if (staff == null) return null;
        return MapModelToDTO(staff);
    }


    public async Task<List<StaffResponseDTO>> GetByFuneralHomeIdAsync(int funeralHomeId)
    {

        var funeralHome = await _funeralHomeRepository.GetByIdAsync(funeralHomeId);

        if (funeralHome == null)
        {
            // Retornamos null para que el controller lance 404
            return null;
        }


        var staffList = await _staffRepository.GetByFuneralHomeIdAsync(funeralHomeId);


        var dtoList = new List<StaffResponseDTO>();

        foreach (var s in staffList)
        {
            dtoList.Add(MapModelToDTO(s));
        }

        return dtoList;
    }

    public async Task<int> CreateAsync(StaffCreateDTO dto)
    {
        //si un empleado es admin ignoro la funeralHome y la pongo en null. 
        if (dto.IsAdmin)
        {
            // Regla 1: Los Admins NO pertenecen a ninguna funeraria (Son globales)
            dto.FuneralHomeId = null;
        }
        else//si es staff normal
        {
            //debe tener funeraria
            if (!dto.FuneralHomeId.HasValue)
            {
                throw new ArgumentException("un empleado debe vincularse a una funeraria");
            }

            //cojo valor de funeraria
            //?? Resulta que no se puede pasar int? a una función que es pera int, no hay conversión de tipos nullables. Hay que poner .Value para especificar que coja el valor entero que contiene el nullable. así lo convierte a int sólo si no es null
            var fh = await _funeralHomeRepository.GetByIdAsync(dto.FuneralHomeId.Value);
            if (fh == null) throw new ArgumentException($"No existe la funeraria {dto.FuneralHomeId.Value}");

        }


        if (await _staffRepository.ExistsByEmailAsync(dto.Email))
            throw new ArgumentException($"El email {dto.Email} ya está registrado.");

        if (await _staffRepository.ExistsByDniAsync(dto.DNI))
            throw new ArgumentException($"El DNI {dto.DNI} ya está registrado.");


        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);


        var newStaff = new Staff
        {
            FuneralHomeId = dto.FuneralHomeId,
            Name = dto.Name,
            Email = dto.Email,
            DNI = dto.DNI,
            PasswordHash = passwordHash,
            IsAdmin = dto.IsAdmin
        };

        return await _staffRepository.AddAsync(newStaff);
    }

    public async Task<bool> UpdateAsync(StaffUpdateDTO dto)
    {
        //existe el staff a actualizar
        var existing = await _staffRepository.GetByIdAsync(dto.Id);
        if (existing == null) return false;

        //si cambian las cosas que deben ser unicas valido que sean unicas
        if (existing.Email != dto.Email)
        {
            if (await _staffRepository.ExistsByEmailAsync(dto.Email, excludeId: dto.Id))
                throw new ArgumentException($"El email {dto.Email} ya está en uso.");
        }

        if (existing.DNI != dto.DNI)
        {
            if (await _staffRepository.ExistsByDniAsync(dto.DNI, excludeId: dto.Id))
                throw new ArgumentException($"El DNI {dto.DNI} ya está en uso.");
        }



        var updatedStaff = new Staff
        {
            Id = dto.Id,
            FuneralHomeId = existing.FuneralHomeId, //hay que mantener el de al base de datos
            Name = dto.Name,
            Email = dto.Email,
            DNI = dto.DNI
        };

        return await _staffRepository.UpdateAsync(updatedStaff);
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _staffRepository.GetByIdAsync(id);
        if (existing == null) return false;

        return await _staffRepository.DeleteAsync(id);
    }

    private StaffResponseDTO MapModelToDTO(Staff model)
    {
        return new StaffResponseDTO
        {
            Id = model.Id,
            FuneralHomeId = model.FuneralHomeId,
            Name = model.Name,
            Email = model.Email,
            DNI = model.DNI
        };
    }


}