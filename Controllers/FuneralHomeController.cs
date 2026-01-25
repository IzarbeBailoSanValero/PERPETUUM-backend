using Microsoft.AspNetCore.Mvc;
using PERPETUUM.Services;
using PERPETUUM.DTOs;

namespace PERPETUUM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuneralHomeController : ControllerBase
{
    private readonly IFuneralHomeService _service;
    private readonly ILogger<FuneralHomeController> _logger;

    public FuneralHomeController(IFuneralHomeService service, ILogger<FuneralHomeController> logger)
    {
        _service = service;
        _logger = logger;
    }


    [HttpGet]
    public async Task<ActionResult<List<FuneralHomeResponseDTO>>> GetAll()
    {
        try
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al obtener el listado de funerarias.");
            return StatusCode(500, new { message = "Ocurrió un error interno al recuperar los datos." });
        }
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<FuneralHomeResponseDTO>> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            
            if (result == null) 
            //sintaxis:a los codigos de estado  le puedes meter dentro un objeto con un body, escribimos un mensaje de lo que no se ha encontrado
                return NotFound(new { message = $"No se encontró la funeraria con ID {id}" });
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error crítico al obtener la funeraria {id}.");
            return StatusCode(500, new { message = "Ocurrió un error interno al recuperar el recurso." });
        }
    }


    [HttpPost]
    public async Task<ActionResult> Create([FromBody] FuneralHomeCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            int newId = await _service.CreateAsync(dto);

            var createdItem = await _service.GetByIdAsync(newId);
            
            return CreatedAtAction(nameof(GetById), new { id = newId }, createdItem);
        }
        catch (ArgumentException ex)
        {
            // Regla de negocio violada  CIF duplicado -> 409 
            _logger.LogWarning($"Intento de creación fallido (Conflicto): {ex.Message}");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al crear funeraria.");
            return StatusCode(500, new { message = "Ocurrió un error interno al procesar la creación." });
        }
    }

  
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] FuneralHomeUpdateDTO dto)
    {
        if (id != dto.Id) 
            return BadRequest(new { message = "El ID de la URL no coincide con el cuerpo de la petición." });
        
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            bool updated = await _service.UpdateAsync(dto);

            if (!updated) 
                return NotFound(new { message = $"No se encontró la funeraria con ID {id} para actualizar." });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Intento de actualización fallido (Conflicto) ID {id}: {ex.Message}");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error crítico al actualizar funeraria {id}.");
            return StatusCode(500, new { message = "Ocurrió un error interno al procesar la actualización." });
        }
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _service.DeleteAsync(id);

            if (!deleted) 
                return NotFound(new { message = $"No se encontró la funeraria con ID {id} para borrar." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error crítico al borrar funeraria {id}.");
            return StatusCode(500, new { message = "Ocurrió un error interno al intentar eliminar el recurso." });
        }
    }
}