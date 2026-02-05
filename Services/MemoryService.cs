using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Repositories;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Services;

public class MemoryService : IMemoryService
{
    private readonly IMemoryRepository _repository;
    private readonly ILogger<MemoryService> _logger;

    public MemoryService(IMemoryRepository repository, ILogger<MemoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<MemoryResponseDTO>> GetByDeceasedIdAsync(int deceasedId, bool onlyApproved)
    {
        _logger.LogInformation($"Service: Obteniendo memorias para difunto {deceasedId}. Solo aprobadas: {onlyApproved}");

        List<Memory> memories;

        if (onlyApproved)
        {
            memories = await _repository.GetApprovedByDeceasedIdAsync(deceasedId);
        }
        else
        {
            memories = await _repository.GetByDeceasedIdAsync(deceasedId);
        }

        var responseDtos = new List<MemoryResponseDTO>();

        foreach (var memory in memories)
        {
            responseDtos.Add(MapToDTO(memory));
        }

        return responseDtos;
    }



    public async Task<MemoryResponseDTO?> GetByIdAsync(int id)
    {
        var memory = await _repository.GetByIdAsync(id);
        if (memory == null) return null;
        return MapToDTO(memory);
    }










    //no infiero el tipo en el backend porque le usuario debe poder elegir anécdota vs condolencia
    public async Task<int> AddMemoryAsync(MemoryCreateDTO dto, int currentUserId)
    {
        //valido que si es de tipo condolencia o anécdota exista texto
        if ((dto.Type == 1 || dto.Type == 2) && string.IsNullOrWhiteSpace(dto.TextContent))
        {
            throw new ArgumentException("El contenido de texto es obligatorio para condolencias y anécdotas.");
        }

        if (dto.Type == 3 && string.IsNullOrWhiteSpace(dto.MediaURL))
        {
            throw new ArgumentException("Es necesario incluir contenido visual.");
        }

        _logger.LogInformation($"Service: Creando nueva memoria para difunto {dto.DeceasedId}");

        var memory = new Memory
        {
            DeceasedId = dto.DeceasedId,
            UserId = currentUserId,
            Type = (MemoryType)dto.Type, // en el model es MemoryType, en el dtocreate es un int
            Status = MemoryStatus.Pending, //por defecto pending
            TextContent = dto.TextContent,
            MediaURL = dto.MediaURL,
            AuthorRelation = dto.AuthorRelation,
            CreatedDate = DateTime.UtcNow
        };

        int newMemoryId = await _repository.AddAsync(memory);
        _logger.LogInformation($"Memoria añadida con éxito. El id  es {newMemoryId}");
        return newMemoryId;


    }

    public async Task<bool> DeleteMemoryAsync(int id)
    {
        _logger.LogInformation($"Service: Ejecutando borrado  de memoria {id}");

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        return await _repository.DeleteAsync(id);
    }


    public async Task<bool> UpdateStatusAsync(int id, MemoryStatus status)
    {
        _logger.LogInformation($"Service: Actualizando estado de memoria {id} a {status}");

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        return await _repository.UpdateStatusAsync(id, status);
    }


    private MemoryResponseDTO MapToDTO(Memory m)
    {
        return new MemoryResponseDTO
        {
            Id = m.Id,
            CreatedDate = m.CreatedDate,
            Type = m.Type.ToString(),  //transforma el enum de numero a estado     
            Status = m.Status.ToString(),
            TextContent = m.TextContent,
            MediaURL = m.MediaURL,
            AuthorRelation = m.AuthorRelation,
            DeceasedId = m.DeceasedId,
            UserId = m.UserId
        };
    }


}

