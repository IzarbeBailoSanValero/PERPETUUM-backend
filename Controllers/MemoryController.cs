using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Services;
using System.Security.Claims;

namespace PERPETUUM.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MemoryController : ControllerBase
{
    private readonly IMemoryService _memoryService;
    private readonly ILogger<MemoryController> _logger;
    private readonly IDeceasedService _deceasedService;

    public MemoryController(IMemoryService service, ILogger<MemoryController> logger, IDeceasedService deceasedService)
    {
        _memoryService = service;
        _logger = logger;
        _deceasedService = deceasedService;
    }

    [AllowAnonymous]
    [HttpGet("deceased/{deceasedId}")]
    public async Task<ActionResult<List<MemoryResponseDTO>>> GetByDeceased(int deceasedId, [FromQuery] bool approved = true)
    {
        try
        {
            var list = await _memoryService.GetByDeceasedIdAsync(deceasedId, approved);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener memorias del difunto {DeceasedId}", deceasedId);
            return StatusCode(500, "Ocurrió un error interno al recuperar los datos.");
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Guardian)]
    public async Task<ActionResult<List<MemoryModerationDTO>>> GetPending([FromQuery] int? deceasedId = null)
    {
        try
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            if (User.IsInRole(Roles.Admin))
            {
                var adminFilter = deceasedId.HasValue ? new List<int> { deceasedId.Value } : null;
                var pendingForAdmin = await _memoryService.GetPendingForModerationAsync(adminFilter);
                return Ok(pendingForAdmin);
            }

            var myDeceasedIds = await GetAllowedDeceasedIdsForGuardian(currentUserId.Value);
            if (myDeceasedIds.Count == 0)
            {
                return Ok(new List<MemoryModerationDTO>());
            }

            if (deceasedId.HasValue && !myDeceasedIds.Contains(deceasedId.Value))
            {
                return Forbid();
            }

            var guardianFilter = deceasedId.HasValue ? new List<int> { deceasedId.Value } : myDeceasedIds;
            var pendingForGuardian = await _memoryService.GetPendingForModerationAsync(guardianFilter);
            return Ok(pendingForGuardian);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener memorias pendientes de moderación");
            return StatusCode(500, "Error interno al recuperar pendientes de moderación.");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.StandardUser)]
    public async Task<ActionResult> AddMemory([FromBody] MemoryCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Payload inválido en creación de memoria");
            return BadRequest(ModelState);
        }

        try
        {
            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var newId = await _memoryService.AddMemoryAsync(dto, currentUserId.Value);
            return CreatedAtAction(nameof(GetByDeceased), new { deceasedId = dto.DeceasedId }, new { id = newId });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Regla de negocio inválida al crear memoria: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al crear memoria");
            return StatusCode(500, "Error interno al procesar petición");
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Guardian)]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] MemoryStatusUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Payload inválido al actualizar estado de memoria");
            return BadRequest(ModelState);
        }

        try
        {
            var memoryDTO = await _memoryService.GetByIdAsync(id);
            if (memoryDTO == null)
            {
                return NotFound($"No se encontró la memoria con ID {id}.");
            }

            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            bool canUpdate = User.IsInRole(Roles.Admin);

            if (!canUpdate && User.IsInRole(Roles.Guardian))
            {
                canUpdate = await CanGuardianModerateDeceased(currentUserId.Value, memoryDTO.DeceasedId);
            }

            if (!canUpdate)
            {
                return Forbid();
            }

            var newStatus = (MemoryStatus)dto.Status;
            bool hasBeenUpdated = await _memoryService.UpdateStatusAsync(id, newStatus);

            if (!hasBeenUpdated) return NotFound($"No se encontró la memoria con ID {id}.");

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Regla de negocio inválida al actualizar estado: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de memoria {Id}", id);
            return StatusCode(500, "Error interno al actualizar el estado.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Guardian + "," + Roles.StandardUser)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var memoryDTO = await _memoryService.GetByIdAsync(id);
            if (memoryDTO == null)
            {
                _logger.LogWarning("Intento de eliminación de memoria inexistente {Id}", id);
                return NotFound();
            }

            int? currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            bool canDelete = false;

            if (User.IsInRole(Roles.Admin))
            {
                canDelete = true;
            }
            else if (memoryDTO.UserId == currentUserId.Value)
            {
                canDelete = true;
            }
            else if (User.IsInRole(Roles.Guardian))
            {
                canDelete = await CanGuardianModerateDeceased(currentUserId.Value, memoryDTO.DeceasedId);
            }

            if (!canDelete)
            {
                return Forbid();
            }

            var hasBeenDeleted = await _memoryService.DeleteMemoryAsync(id);
            if (!hasBeenDeleted)
            {
                _logger.LogWarning("No se pudo eliminar memoria {Id}", id);
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al eliminar memoria {Id}", id);
            return StatusCode(500, "Error interno al procesar petición");
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return null;
        return int.TryParse(userIdClaim.Value, out var parsedId) ? parsedId : null;
    }

    private async Task<List<int>> GetAllowedDeceasedIdsForGuardian(int guardianId)
    {
        var myDeceased = await _deceasedService.GetByGuardianIdAsync(guardianId);
        var ids = new List<int>(myDeceased.Count);

        foreach (var deceased in myDeceased)
        {
            ids.Add(deceased.Id);
        }

        return ids;
    }

    private async Task<bool> CanGuardianModerateDeceased(int guardianId, int deceasedId)
    {
        var myDeceasedIds = await GetAllowedDeceasedIdsForGuardian(guardianId);
        return myDeceasedIds.Contains(deceasedId);
    }
}
