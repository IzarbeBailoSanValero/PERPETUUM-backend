using PERPETUUM.Repositories;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Services;

public class FuneralHomeService : IFuneralHomeService
{
    private readonly IFuneralHomeRepository _repository;
    private readonly ILogger<FuneralHomeService> _logger;

    public FuneralHomeService(IFuneralHomeRepository repository, ILogger<FuneralHomeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }



    public async Task<List<FuneralHomeResponseDTO>> GetAllAsync()
    {
        _logger.LogInformation("Service: Obteniendo todas las funerarias");
        var models = await _repository.GetAllAsync();

        var dtos = new List<FuneralHomeResponseDTO>();
        foreach (var model in models)
        {
            dtos.Add(MapModelToDTO(model));
        }
        return dtos;
    }


    public async Task<FuneralHomeResponseDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation($"Service: Buscando funeraria ID {id}");
        var model = await _repository.GetByIdAsync(id);

        if (model == null) return null;

        return MapModelToDTO(model);
    }

    public async Task<int> CreateAsync(FuneralHomeCreateDTO dto)
    {
        _logger.LogInformation("Service: Creando nueva funeraria");


        bool exists = await _repository.ExistsByCifAsync(dto.CIF);
        if (exists)
        {
            throw new ArgumentException($"Ya existe una funeraria registrada con el CIF {dto.CIF}");
        }


        var newFuneralHome = new FuneralHome
        {
            Name = dto.Name,
            CIF = dto.CIF,
            ContactEmail = dto.ContactEmail,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber
        };


        int newId = await _repository.AddAsync(newFuneralHome);
        _logger.LogInformation($"Funeraria creada con éxito. ID: {newId}");

        return newId;


    }
    public async Task<bool> UpdateAsync(FuneralHomeUpdateDTO dto)
    {
        _logger.LogInformation($"Service: Actualizando funeraria ID {dto.Id}");


        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null)
        {
            _logger.LogWarning($"Intento de editar funeraria inexistente ID {dto.Id}");
            return false;
        }

        // YO IBA A VERIFICAR POR SISTEMA, MEJORA CON IA
        if (existing.CIF != dto.CIF)
        {
            bool exists = await _repository.ExistsByCifAsync(dto.CIF, excludeId: dto.Id);
            if (exists)
            {
                throw new ArgumentException($"El CIF {dto.CIF} ya está en uso por otra funeraria.");
            }
        }


        var updatedFuneralHome = new FuneralHome
        {
            Id = dto.Id,
            Name = dto.Name,
            CIF = dto.CIF,
            ContactEmail = dto.ContactEmail,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber
        };


        return await _repository.UpdateAsync(updatedFuneralHome);
    }



    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation($"Service: Borrando funeraria ID {id}");

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        return await _repository.DeleteAsync(id);
    }




    private FuneralHomeResponseDTO MapModelToDTO(FuneralHome model)
    {
        return new FuneralHomeResponseDTO
        {
            Id = model.Id,
            Name = model.Name,
            CIF = model.CIF,
            ContactEmail = model.ContactEmail,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber
        };
    }

























}