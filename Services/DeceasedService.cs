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



    public Task<int> CreateDeceasedAsync(DeceasedCreateDTO createDTO)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteDeceasedAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<DeceasedResponseDTO>> GetAllDeceasedAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DeceasedResponseDTO?> GetDeceasedProfileAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<MemoryResponseDTO>> GetMemoriesByDeceasedIdAsync(int deceasedId)
    {
        throw new NotImplementedException();
    }

    public Task<List<DeceasedResponseDTO>> SearchDeceasedAsync(DeceasedSearchDTO searchDTO)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateDeceasedAsync(DeceasedUpdateDTO updateDTO)
    {
        throw new NotImplementedException();
    }



    //- - - MÉTODOS PRIVADOS - - - (Idea de chatgpt, me ha gustado, la cojo,  todo más limpio)
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