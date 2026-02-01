using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PERPETUUM.DTOs;
using PERPETUUM.Services;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;

namespace PERPETUUM.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] //Todas las acciones del controlador requieren un usuario autenticado. Si alguien intenta acceder sin token → 401 Unauthorized.
public class MemorialGuardianController : ControllerBase
{
    private readonly IMemorialGuardianService _service;
    private readonly ILogger<MemorialGuardianController> _logger;

    public MemorialGuardianController(IMemorialGuardianService service, ILogger<MemorialGuardianController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Staff + "," + Roles.Admin)] //Si el usuario está autenticado pero no tiene esos roles → 403 Forbidden.
    public async Task<ActionResult> Create([FromBody] GuardianCreateDTO dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var id = await _service.CreateGuardianAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando guardian");
            return StatusCode(500, new { message = "Error interno" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GuardianResponseDTO>> GetById(int id)
    {
        try
        {
            var guardian = await _service.GetGuardianByIdAsync(id);
            if (guardian == null) return NotFound();
            return Ok(guardian);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo guardian {Id}", id);
            return StatusCode(500, new { message = "Error interno" });
        }
    }
}