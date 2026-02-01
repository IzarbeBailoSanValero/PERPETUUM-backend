using Microsoft.AspNetCore.Mvc;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using PERPETUUM.Services;

namespace PERPETUUM.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MemoryController : ControllerBase
{
    private readonly IMemoryService _service;
    private readonly ILogger<MemoryController> _logger;

    public MemoryController(IMemoryService service, ILogger<MemoryController> logger)
    {
        _service = service;
        _logger = logger;
    }


    [HttpGet("deceased/{deceasedId}")]
    public async Task<ActionResult<List<MemoryResponseDTO>>> GetByDeceased(int deceasedId, [FromQuery] bool approved = true) //     Permite añadir ?approved=false en la URL. //el deceased lo coge del nombre de la dirección directamente
    {
        try
        {
            var list = await _service.GetByDeceasedIdAsync(deceasedId, approved);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error en Controller al obtener memorias del difunto {deceasedId}");
            return StatusCode(500, "Ocurrió un error interno al recuperar los datos.");
        }
    }


    [HttpPost]
    public async Task<ActionResult> AddMemory([FromBody] MemoryCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Fallo al validar memoria debido a un formato de datos enviados inválido.");
            return BadRequest(ModelState);
        }
        try
        {
            var newId = await _service.AddMemoryAsync(dto);
            return CreatedAtAction(nameof(GetByDeceased), new { deceasedId = dto.DeceasedId }, new { id = newId });
        }
        catch (ArgumentException ex)
            {
                
                _logger.LogWarning("Error respecto a las reglas de negocio en memory");
                return Conflict(ex.Message); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar create deceased en controller");
                return StatusCode(500, "Error interno al procesar petición");
            }
    }



[HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] int statusInt)
    {
          if (!ModelState.IsValid)
        {
            _logger.LogWarning("Fallo al validar memoria debido a un formato de datos enviados inválido.");
            return BadRequest(ModelState);
        }
        
        try
        {
            var status = (MemoryStatus)statusInt;
            bool hasBeenUpdated = await _service.UpdateStatusAsync(id, status);

            if (!hasBeenUpdated) return NotFound($"No se encontró la memoria con ID {id}.");

            return NoContent(); 
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error respecto a las reglas de negocio en memory: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en Controller al actualizar estado de la memoria {Id}", id);
            return StatusCode(500, "Error interno al actualizar el estado.");
        }
    }


[HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try 
            {
                var hasBeenDeleted = await _service.DeleteMemoryAsync(id);

                if (!hasBeenDeleted)
                {
                    _logger.LogWarning($"Fracaso al eliminar memoria con id {id}, no encontrado en base de datos");
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar delete memory en controller"); 
                return StatusCode(500, "Error interno al procesar petición");
            }
    }

}