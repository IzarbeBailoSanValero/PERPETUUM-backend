using PERPETUUM.Repositories;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Services;

public class DeceasedService : IDeceasedService
{
    private readonly IDeceasedRepository _repository;
    private readonly ILogger<DeceasedService> _logger;
    private readonly IMemoryRepository _memoryRepository;
    

    public DeceasedService(IDeceasedRepository repository, ILogger<DeceasedService> logger, IMemoryRepository memoryRepository)
    {
        _repository = repository;
        _logger = logger;
        _memoryRepository = memoryRepository;
    }

    public async Task<List<DeceasedResponseDTO>> GetAllDeceasedAsync()
    {
        _logger.LogInformation("Service: Obteniendo todos los difuntos");
        var models = await _repository.GetAllAsync();
        var dtos = new List<DeceasedResponseDTO>();

        foreach (var m in models)
        {
            dtos.Add(MapModelToDTO(m, includeMemories: false));
        }
        return dtos;
    }

   public async Task<DeceasedResponseDTO?> GetDeceasedProfileAsync(int id) //solo trae con memorias aprobadas
{
    _logger.LogInformation("Service: Obteniendo perfil público del difunto {Id}", id);


    var deceasedModel = await _repository.GetByIdAsync(id);

    if (deceasedModel == null) return null;

    //pongo include a false para que no traiga todas las memorias, luego pillo yo las aprobadas con el metodo de memorias aprobadas por difunto
    var dto = MapModelToDTO(deceasedModel, includeMemories: false);

    
    var approvedMemories = await _memoryRepository.GetApprovedByDeceasedIdAsync(id);

    dto.Memories = approvedMemories.Select(m => MapMemoryToDTO(m)).ToList();

    return dto;
}




public async Task<List<DeceasedSummaryDTO>> GetByGuardianIdAsync(int guardianId)
{
    _logger.LogInformation($"Service: Obteniendo lista de difuntos por guardián para el guardian con id {guardianId}");
    return await _repository.GetByGuardianIdAsync(guardianId);
}

    public async Task<int> CreateDeceasedAsync(DeceasedCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PhotoURL))
            throw new ArgumentException("La foto es obligatoria (indica PhotoURL o sube una imagen).");

        ValidateDates(dto.BirthDate, dto.DeathDate);

        bool exists = await _repository.ExistsByDniAsync(dto.Dni);
        if (exists)
        {
            throw new ArgumentException($"Ya existe un difunto registrado con el DNI {dto.Dni}");
        }

        var newDeceased = new Deceased
        {
            FuneralHomeId = dto.FuneralHomeId,
            GuardianId = dto.GuardianId,
            StaffId = dto.StaffId,
            Dni = dto.Dni, 
            Name = dto.Name,
            Epitaph = dto.Epitaph,
            Biography = dto.Biography,
            PhotoURL = dto.PhotoURL,
            BirthDate = dto.BirthDate,
            DeathDate = dto.DeathDate
        };

        int newDeceasedId = await _repository.AddAsync(newDeceased);
        _logger.LogInformation($"Difunto añadido con éxito. El id del nuevo difunto es {newDeceasedId}");

        return newDeceasedId;
    }

public async Task<bool> UpdateDeceasedAsync(DeceasedUpdateDTO dto)
{
    var existing = await _repository.GetByIdAsync(dto.Id);
    if (existing == null) return false;

    ValidateDates(dto.BirthDate, dto.DeathDate);

   
    if (existing.Dni != dto.Dni)
    {
     
        bool exists = await _repository.ExistsByDniAsync(dto.Dni, excludeId: dto.Id);
        
        if (exists)
        {
            throw new ArgumentException($"El DNI {dto.Dni} ya está en uso por otro difunto.");
        }
    }
    
    var updatedDeceased = new Deceased
    {
        Id = dto.Id,
        FuneralHomeId = dto.FuneralHomeId,
        GuardianId = dto.GuardianId,
        StaffId = dto.StaffId,
        Dni = dto.Dni, // Asignamos el nuevo DNI
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

        bool hasBeenDeleted = await _repository.DeleteAsync(id);
        return hasBeenDeleted;
    }

    public async Task<List<DeceasedResponseDTO>> SearchDeceasedAsync(DeceasedSearchDTO searchDTO)
    {
        if (!string.IsNullOrEmpty(searchDTO.Name)) searchDTO.Name = searchDTO.Name.Trim();

        var models = await _repository.SearchAsync(searchDTO);
        var dtos = new List<DeceasedResponseDTO>();

        foreach (var m in models)
        {
            dtos.Add(MapModelToDTO(m, includeMemories: false));
        }
        return dtos;
    }

   

    //- - - MÉTODOS PRIVADOS - - - 
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
        GuardianId = model.GuardianId,
        FuneralHomeId = model.FuneralHomeId,
        Dni = model.Dni,
        Name = model.Name,
        Epitaph = model.Epitaph,
        Biography = model.Biography,
        PhotoURL = model.PhotoURL,
        BirthDate = model.BirthDate,
        DeathDate = model.DeathDate,
        Memories = new List<MemoryResponseDTO>() // Lista vacía por defecto
    };

    
    // En GetDeceasedProfileAsync le pasamos 'false para hacerlo nosotros manualmente después
    if (includeMemories && model.Memories != null)
    {
        foreach (var mem in model.Memories)
        {
            dto.Memories.Add(MapMemoryToDTO(mem));
        }
    }
    return dto;
}

   private MemoryResponseDTO MapMemoryToDTO(Memory mem)
{
    return new MemoryResponseDTO
    {
        Id = mem.Id,
        CreatedDate = mem.CreatedDate,
        
        // con toString() para que sale la palabra del enum en vez del número : IA
        Type = mem.Type.ToString(), 
        Status = mem.Status.ToString(),
        
        TextContent = mem.TextContent,
        MediaURL = mem.MediaURL,
        AuthorRelation = mem.AuthorRelation,
        UserId = mem.UserId,
        DeceasedId = mem.DeceasedId
    };
}
}