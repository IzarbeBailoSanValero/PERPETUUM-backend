using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PERPETUUM.DTOs;
using PERPETUUM.Services;
using PERPETUUM.Models;
using System.Security.Claims;


namespace PERPETUUM.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] //si no loggeado --> 401 Unauthorized
public class MemorialGuardianController : ControllerBase
{
    private readonly IMemorialGuardianService _service;
    private readonly ILogger<MemorialGuardianController> _logger;
    private readonly IAuthService _authService;
     private readonly IStaffService _staffService;

    public MemorialGuardianController(IMemorialGuardianService service, ILogger<MemorialGuardianController> logger, IAuthService authService,IStaffService staffService)
    {
        _service = service;
         _staffService = staffService;
        _logger = logger;
        _authService = authService;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Staff + "," + Roles.Admin+ "," + Roles.Guardian)] //2º CAPA BARRERA --> en la tercera hay que verificar guardian propio y staff de misma funeraria
    public async Task<ActionResult<GuardianResponseDTO>> GetById(int id)
    {
        try
        {
            //recupero guardian
            var guardian = await _service.GetGuardianByIdAsync(id);
            if (guardian == null) return NotFound();


            //3º capa: si es staff --> debe pertenecer a la misma funeraria
            //buscamos el funeralhomeId del guardian para compararlo con el de la funeraria peticionaria
           
           
            if (User.IsInRole(Roles.Staff))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int currentUserId = int.Parse(userIdClaim.Value);

                //perfil del empleado
                var staffProfile = await _staffService.GetByIdAsync(currentUserId);

                // Si el empleado no existe o su funeraria no coincide 
                if (staffProfile == null || staffProfile.FuneralHomeId != guardian.FuneralHomeId)
                {
                    return Forbid(); 
                }
            }
            
           else if (User.IsInRole(Roles.Guardian))
            {
                
                if (!_authService.HasAccessToResource(id, User))
                {
                    return Forbid(); //guardian intenta ver perfil de otro guardian
                } 
            }
            
            //ha pasado filtros
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

            //comprobar que un staff no pueda crear un guardian para otra funeraria (mejora IA)
                if (User.IsInRole(Roles.Staff))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null) return Unauthorized();
                    int currentUserId = int.Parse(userIdClaim.Value);
    
                    //perfil del empleado
                    var staffProfile = await _staffService.GetByIdAsync(currentUserId);
    
                    // Si el empleado no existe o su funeraria no coincide 
                    if (staffProfile == null || staffProfile.FuneralHomeId != dto.FuneralHomeId)
                    {
                        return Forbid(); 
                    }
                }

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