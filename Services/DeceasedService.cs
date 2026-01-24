//using
using PERPETUUM.Repositories;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Services;

public class DeceasedService : IDeceasedService
{
    private readonly IDeceasedRepository _repository;
    private readonly ILogger<DeceasedService> _logger;

    public DeceasedService(IDeceasedRepository repository, ILogger<DeceasedService> logger)
    {
        _repository = repository;
        _logger = logger;
    }



    public async Task<List<DeceasedResponseDTO>> GetAllDeceasedAsync()
    {
       _logger.LogInformation("Service: Obteniendo todos los difuntos");
        var models = await _repository.GetAllAsync();
        var dtos = new List<DeceasedResponseDTO>();

        foreach (var m in models)
        {
            dtos.Add(MapModelToDTO(m, includeMemories: false)); //se puede poner así, es lo mismo que (m,false) pero se lee mejor, lo dejo así
        }
        return dtos;
    }



    public async Task<DeceasedResponseDTO?> GetDeceasedProfileAsync(int id)
    {
       _logger.LogInformation("Service: Obteniendo difunto por id");
        var model = await _repository.GetByIdAsync(id);

        if (model == null) return null;

        return MapModelToDTO(model, includeMemories: true);
    }



    public async Task<int> CreateDeceasedAsync(DeceasedCreateDTO dto)
    {
        ValidateDates(dto.BirthDate, dto.DeathDate);

        var newDeceased = new Deceased
        {
            FuneralHomeId = dto.FuneralHomeId,
            GuardianId = dto.GuardianId,
            StaffId = dto.StaffId,
            Name = dto.Name,
            Epitaph = dto.Epitaph,
            Biography = dto.Biography,
            PhotoURL = dto.PhotoURL,
            BirthDate = dto.BirthDate,
            DeathDate = dto.DeathDate
        };

        int newDeceasedId =  await _repository.AddAsync(newDeceased);
         _logger.LogInformation($"difunto añadido con éxito. El id del nuevo difunto es {newDeceasedId}");

        return newDeceasedId;
    }

    public async Task<bool> UpdateDeceasedAsync(DeceasedUpdateDTO dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null) return false;

        ValidateDates(dto.BirthDate, dto.DeathDate);

        var updatedDeceased = new Deceased
        {
            Id = dto.Id,
            FuneralHomeId = dto.FuneralHomeId,
            GuardianId = dto.GuardianId,
            StaffId = dto.StaffId,
            Name = dto.Name,
            Epitaph = dto.Epitaph,
            Biography = dto.Biography,
            PhotoURL = dto.PhotoURL,
            BirthDate = dto.BirthDate,
            DeathDate = dto.DeathDate
        };

        bool hasBeenUpdated = await _repository.UpdateAsync(updatedDeceased);
        return hasBeenUpdated;
    }


   public async Task<bool> DeleteDeceasedAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        bool hasBeenDeleted =await _repository.DeleteAsync(id);
        return hasBeenDeleted;
    }


   
public async Task<List<DeceasedResponseDTO>> SearchDeceasedAsync(DeceasedSearchDTO searchDTO)
    {
        if (!string.IsNullOrEmpty(searchDTO.Name)) searchDTO.Name = searchDTO.Name.Trim(); //quita espacios delante y detrás par aenviar bine el dto

        var models = await _repository.SearchAsync(searchDTO);
        var dtos = new List<DeceasedResponseDTO>();

        foreach (var m in models)
        {
            dtos.Add(MapModelToDTO(m, includeMemories: false));
        }
        return dtos;
    }

public async Task<List<MemoryResponseDTO>> GetMemoriesByDeceasedIdAsync(int deceasedId)
    {
        var deceased = await _repository.GetByIdAsync(deceasedId);
        if (deceased == null)
        {
            throw new ArgumentException($"El difunto con ID {deceasedId} no existe.");
        }

        var memories = await _repository.GetMemoriesByDeceasedIdAsync(deceasedId);
        var memoryDTOs = new List<MemoryResponseDTO>();

        foreach (var mem in memories)
        {
            memoryDTOs.Add(MapMemoryToDTO(mem));
        }
        return memoryDTOs;
    }


    //- - - MÉTODOS PRIVADOS - - - (Idea de chatgpt, me ha gustado, los cojo con su estructura, muy buena,  todo más limpio)
    private void ValidateDates(DateTime birth, DateTime death)
    {
        if (birth > death) throw new ArgumentException("Nacimiento posterior a fallecimiento.");
        if (death > DateTime.Now) throw new ArgumentException("Fecha de fallecimiento futura.");
    }

    private DeceasedResponseDTO MapModelToDTO(Deceased model, bool includeMemories)
    {
        var dto = new DeceasedResponseDTO
        {
            Id = model.Id,
            Name = model.Name,
            Epitaph = model.Epitaph,
            Biography = model.Biography,
            PhotoURL = model.PhotoURL,
            BirthDate = model.BirthDate,
            DeathDate = model.DeathDate
        };

        if (includeMemories && model.Memories != null)
        {
            dto.Memories = new List<MemoryResponseDTO>();
            foreach (var mem in model.Memories)
            {
                dto.Memories.Add(MapMemoryToDTO(mem));
            }
        }
        return dto;
    }

private MemoryResponseDTO MapMemoryToDTO(Memory mem)
{
    string typeString;
    switch (mem.Type)
    {
        case 1:
            typeString = "Condolence";
            break;
        case 2:
            typeString = "Photo";
            break;
        case 3:
            typeString = "Anecdote";
            break;
        default:
            typeString = "Unknown";
            break;
    }

    string statusString;
    switch (mem.Status)
    {
        case 0:
            statusString = "Pending";
            break;
        case 1:
            statusString = "Approved";
            break;
        case 2:
            statusString = "Rejected";
            break;
        default:
            statusString = "Unknown";
            break;
    }

    return new MemoryResponseDTO
    {
        Id = mem.Id,
        CreatedDate = mem.CreatedDate,
        Type = typeString,
        Status = statusString,
        TextContent = mem.TextContent,
        MediaURL = mem.MediaURL,
        AuthorRelation = mem.AuthorRelation,
        UserId = mem.UserId,
        DeceasedId = mem.DeceasedId
    };
}







}