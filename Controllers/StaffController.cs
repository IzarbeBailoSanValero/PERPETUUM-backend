using Microsoft.AspNetCore.Mvc;
using PERPETUUM.Services;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using Microsoft.AspNetCore.Authorization;

namespace PERPETUUM.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] //1º seguridad : min loggeado
public class StaffController : ControllerBase
{
    private readonly IStaffService _service;
    private readonly ILogger<StaffController> _logger;
    private readonly IAuthService _authService;

    public StaffController(IStaffService service, ILogger<StaffController> logger, IAuthService authService)
    {
        _service = service;
        _logger = logger;
        _authService = authService;
        
    }

    
    // GET: api/staff/5
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Staff)]
    //2º seguridad: autorizo solo a staff y admin
    public async Task<ActionResult<StaffResponseDTO>> GetById(int id)
    {
        try
        {
            if (!_authService.HasAccessToResource(id, User)) return Forbid(); //3º seguridad, una vez que es seguro que se ha metido un admin o staff, podemos ver por id
            var result = await _service.GetByIdAsync(id);
            if (result == null) 
                return NotFound(new { message = $"No se encontró el empleado con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error crítico al obtener staff {id}.");
            return StatusCode(500, new { message = "Error interno al recuperar el recurso." });
        }
    }

    
    [HttpGet("funeralhome/{funeralHomeId}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Staff)] //un trabajador puede ver a sus compis a grandes rasgos
    public async Task<ActionResult<List<StaffResponseDTO>>> GetByFuneralHome(int funeralHomeId)
    {
        try
        {
            //TODO: // Aquí podríamos validar que el Staff pertenezca a esa funeraria mirando su Token
            
            var list = await _service.GetByFuneralHomeIdAsync(funeralHomeId);
            
            
            if (list == null)
                return NotFound(new { message = $"La funeraria {funeralHomeId} no existe." });

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener staff de la funeraria {funeralHomeId}.");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }



    [HttpPost]
    [Authorize(Roles = Roles.Admin)]//solo podrás postear trabajadores si eres admin. para ello primero te loggeas como admin, consigues el token. cuando haces la peticion post se pone el token de admin y deja. 
    public async Task<ActionResult> Create([FromBody] StaffCreateDTO dto)
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
            _logger.LogWarning($"Conflicto al crear staff: {ex.Message}");
            return Conflict(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al crear staff.");
            return StatusCode(500, new { message = "Error interno al procesar la creación." });
        }
    }

    
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] StaffUpdateDTO dto)
    {
        if (id != dto.Id) 
            return BadRequest(new { message = "El ID de la URL no coincide con el cuerpo." });
        
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            bool updated = await _service.UpdateAsync(dto);

            if (!updated) 
                return NotFound(new { message = $"No se encontró el empleado con ID {id}." });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Conflicto al actualizar staff {id}: {ex.Message}");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error interno al actualizar staff {id}.");
            return StatusCode(500, new { message = "Error interno al procesar la actualización." });
        }
    }

    
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _service.DeleteAsync(id);

            if (!deleted) 
                return NotFound(new { message = $"No se encontró el empleado con ID {id}." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error interno al borrar staff {id}.");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }
}