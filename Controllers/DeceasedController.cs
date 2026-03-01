using Microsoft.AspNetCore.Mvc;
using PERPETUUM.Services;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace PERPETUUM.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeceasedController : ControllerBase
    {
        private readonly IDeceasedService _deceasedService;
        private readonly ILogger<DeceasedController> _logger;
        private readonly IStaffService _staffService;

        public DeceasedController(IDeceasedService deceasedService, ILogger<DeceasedController> logger, IStaffService staffService)
        {
            _deceasedService = deceasedService;
            _staffService = staffService;
            _logger = logger;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<DeceasedResponseDTO>>> GetAllDeceased()
        {
            try
            {
                List<DeceasedResponseDTO> allDeceased = await _deceasedService.GetAllDeceasedAsync();

                _logger.LogInformation("Éxito en petición get all al deceased controller");

                return Ok(allDeceased);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar get all en deceased controller");
                return StatusCode(500, "Error interno al procesar petición");
            }
        }



        [HttpGet("{deceasedId}")]
        [AllowAnonymous]
        public async Task<ActionResult<DeceasedResponseDTO>> GetDeceased(int deceasedId)
        {
            try
            {
                DeceasedResponseDTO? deceased = await _deceasedService.GetDeceasedProfileAsync(deceasedId); //no ponemos form query porque eso se pone cuando los parametros vienen como ?atr=value... aquí ponemos directamente en la url el vlaor, a´si que al llamar al controlador {deceasedId se autoasigna}

                if (deceased == null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Éxito en petición get deceased al deceased controller");

                return Ok(deceased);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar get deceased en controller");
                //esta versión de devolución de error tiene un mensaje plano, e smejor la de funeral home porque e sun objeto json. esto permite mejor gestinon para el front. lo dejo para trner las dos versiones
                return StatusCode(500, "Error interno al procesar petición");
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult> Search([FromQuery] DeceasedSearchDTO searchDTO)
        {
            try
            {
                List<DeceasedResponseDTO> result = await _deceasedService.SearchDeceasedAsync(searchDTO);

                //Parámetros de paginación
                int pageSize = 9; //el del Frontend
                int totalItems = result.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // 3. Recorto la lista aquí para enviar unos pocos registros y no todos --> pagin en memoria
                int pageNumber = 1; // por defecto

                var paginatedResult = result
                    .Skip((pageNumber - 1) * pageSize)          //Omite los elementos de las páginas anteriores.
                    .Take(pageSize)                             //Toma solo los elementos que caben en una página o sino los que haya
                    .ToList();                                   //  Convierte el resultado en una lista normal para devolverla.


                return Ok(new
                {
                    Items = paginatedResult,
                    TotalPages = totalPages > 0 ? totalPages : 1,
                    TotalCount = totalItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al buscar difuntos");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        //FILTROS validacion mejorados con IA

        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Staff)]
        public async Task<ActionResult<DeceasedResponseDTO>> CreateDeceased([FromBody] DeceasedCreateDTO deceasedDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Fallo al validar difunto debido a un formato de datos enviados inválido.");
                return BadRequest(ModelState);
            }

            try
            {

                if (User.IsInRole(Roles.Staff))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null) return Unauthorized();
                    var currentUserId = int.Parse(userIdClaim.Value);


                    var staffProfile = await _staffService.GetByIdAsync(currentUserId);

                    if (staffProfile == null || staffProfile.FuneralHomeId != deceasedDTO.FuneralHomeId)
                    {
                        _logger.LogWarning($"Staff {currentUserId} intentó crear un difunto para la Funeraria {deceasedDTO.FuneralHomeId} sin permiso.");
                        return Forbid(); // 403 Forbidden
                    }
                }


                int newId = await _deceasedService.CreateDeceasedAsync(deceasedDTO);

                //cojo elemento para devolverlo en el createAdAction
                var createdDeceased = await _deceasedService.GetDeceasedProfileAsync(newId);


                return CreatedAtAction(nameof(GetDeceased), new { deceasedId = newId }, createdDeceased);
            }
            catch (ArgumentException ex)
            {

                _logger.LogWarning("Error respecto a las reglas de negocio en create");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar create deceased en controller");
                return StatusCode(500, "Error interno al procesar petición");
            }
        }


        [HttpPut("{deceasedId}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Staff + "," + Roles.Guardian)]
        public async Task<IActionResult> UpdateDeceased(int deceasedId, [FromBody] DeceasedUpdateDTO deceasedDto)
        {
            if (deceasedId != deceasedDto.Id)
            {
                _logger.LogWarning("No coincide el id del objeto a actualizar y el de la petición.");
                return BadRequest("No coincide el id del objeto a actualizar y el de la petición.");
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var currentlyUserId = int.Parse(userIdClaim.Value);

                var deceased = await _deceasedService.GetDeceasedProfileAsync(deceasedId);
                if (deceased == null)
                {
                    return NotFound($"No se encontró difunto con ID {deceasedId}.");
                }

                bool canUpdate = false;

                if (User.IsInRole(Roles.Admin))
                {
                    canUpdate = true;
                }
                else if (User.IsInRole(Roles.Staff))
                {
                    var staffProfile = await _staffService.GetByIdAsync(currentlyUserId);
                    if (staffProfile != null && staffProfile.FuneralHomeId == deceased.FuneralHomeId)
                    {
                        canUpdate = true;
                    }
                }
                else if (User.IsInRole(Roles.Guardian))
                {
                    if (deceased.GuardianId == currentlyUserId)
                    {
                        canUpdate = true;
                    }
                }

                if (!canUpdate)
                {
                    return Forbid();
                }

                // Actualizar solo campos no nulos
                if (deceasedDto.Name != null) deceased.Name = deceasedDto.Name;
                if (deceasedDto.Dni != null) deceased.Dni = deceasedDto.Dni;
                if (deceasedDto.GuardianId.HasValue) deceased.GuardianId = deceasedDto.GuardianId.Value;
                if (deceasedDto.FuneralHomeId.HasValue) deceased.FuneralHomeId = deceasedDto.FuneralHomeId.Value;
                if (deceasedDto.Biography != null) deceased.Biography = deceasedDto.Biography;
                if (deceasedDto.PhotoURL != null) deceased.PhotoURL = deceasedDto.PhotoURL;
                if (deceasedDto.Epitaph != null) deceased.Epitaph = deceasedDto.Epitaph;
                if (deceasedDto.BirthDate.HasValue) deceased.BirthDate = deceasedDto.BirthDate.Value;
                if (deceasedDto.DeathDate.HasValue) deceased.DeathDate = deceasedDto.DeathDate.Value;

                bool hasBeenUpdated = await _deceasedService.UpdateDeceasedAsync(deceasedDto);

                if (hasBeenUpdated)
                {
                    _logger.LogInformation($"Éxito al actualizar el difunto con id {deceasedId}");
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning($"Fracaso al actualizar el difunto con id {deceasedId}");
                    return NotFound($"No encontrado difunto con ID {deceasedId}. Fallo al actualizar");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Error respecto a las reglas de negocio en update");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar update deceased en controller");
                return StatusCode(500, "Error interno al procesar petición");
            }
        }

        [HttpDelete("{deceasedId}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Staff)] //si el familiar quiere borrar, deberá contactar con la funeraria o perpetuum. Si es staff, debe ser de la misma funeraria

        public async Task<IActionResult> DeleteDeceased(int deceasedId)
        {
            try
            {
                // difunto
                var deceased = await _deceasedService.GetDeceasedProfileAsync(deceasedId);
                if (deceased == null) return NotFound();

                //gestión de permisos: admin -->staff misma funeraria
                bool canDelete = false;

                if (User.IsInRole(Roles.Admin))
                {
                    canDelete = true;
                }

                else if (User.IsInRole(Roles.Staff))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null) return Unauthorized();
                    var currentUserId = int.Parse(userIdClaim.Value);

                    var staffProfile = await _staffService.GetByIdAsync(currentUserId);

                    if (staffProfile != null && staffProfile.FuneralHomeId == deceased.FuneralHomeId)
                    {
                        canDelete = true;
                    }
                }

                if (!canDelete) return Forbid();

                // ejecución del borrado
                var hasBeenDeleted = await _deceasedService.DeleteDeceasedAsync(deceasedId);

                if (!hasBeenDeleted)
                {
                    _logger.LogWarning($"Fracaso al eliminar el difunto con id {deceasedId}");
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar delete deceased en controller");
                return StatusCode(500, "Error interno al procesar petición");
            }
        }


    }
}
