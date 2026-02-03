using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PERPETUUM.DTOs;
using PERPETUUM.Services;
using PERPETUUM.Models;


namespace PERPETUUM.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] //si no loggeado --> 401 Unauthorized
public class MemorialGuardianController : ControllerBase
{
    private readonly IMemorialGuardianService _service;
    private readonly ILogger<MemorialGuardianController> _logger;
    private readonly IAuthService _authService;

    public MemorialGuardianController(IMemorialGuardianService service, ILogger<MemorialGuardianController> logger, IAuthService authService)
    {
        _service = service;
        _logger = logger;
        _authService = authService;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Staff + "," + Roles.Admin+ "," + Roles.Guardian)] //2º CAPA BARRERA --> en la tercera hay que verificar guardian propio y staff de misma funeraria
    public async Task<ActionResult<GuardianResponseDTO>> GetById(int id)
    {
        try
        {
            //3º capa: si es staff --> debe pertenecer a la misma funeraria
            //buscamos el funeralhomeId del guardian para compararlo con el de la funeraria peticionaria
            var guardian = await _service.GetGuardianByIdAsync(id);
            if (guardian == null) return NotFound();
           
            if (User.IsInRole(Roles.Staff))
            {
                 //cojo su fhId y lo comparo con el del recurso
                var funeralHomeIdFromClaim = User.FindFirst("FuneralHomeId");
                if (funeralHomeIdFromClaim == null) return Unauthorized();

                if(funeralHomeIdFromClaim.Value != guardian.FuneralHomeId.ToString()) return Forbid();//comparo con el json --> tostring
            }
            
            else //es guardian --> si no pasa la criba admin/himself lo bloqueamos
            {
               if (!_authService.HasAccessToResource(id, User))
                {
                    return Forbid(); //no puede ver perfiles no suyos
                } 
            }
            
            //si ha pasado todos los filtros, retornar
            return Ok(guardian);
        }


        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo guardian {Id}", id);
            return StatusCode(500, new { message = "Error interno" });
        }
    }



    [HttpPost]
    [Authorize(Roles = Roles.Staff + "," + Roles.Admin)] //no tiene esos roles → 403 Forbidden.
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

}