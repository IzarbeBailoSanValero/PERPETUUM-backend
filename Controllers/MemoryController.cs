using Microsoft.AspNetCore.Mvc;
using PERPETUUM.DTOs;
using PERPETUUM.Models; //para utilizar los roles en la protección
using PERPETUUM.Services;
using Microsoft.AspNetCore.Authorization;

namespace PERPETUUM.Controllers;

using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
//hago la proteccion a nivel de función
public class MemoryController : ControllerBase
{
    private readonly IMemoryService _memoryService;
    private readonly ILogger<MemoryController> _logger;
    private readonly IDeceasedService _deceasedService;

    public MemoryController(IMemoryService service, ILogger<MemoryController> logger, IMemorialGuardianService guardianService, IDeceasedService deceasedService)
    {
        _memoryService = service;
        _logger = logger;
        _deceasedService = deceasedService;
    }



    [AllowAnonymous] //se que no hace falta ponerlo porque no he puesto autorize a nivel de controller, pero lo dejo por si en un futuro lo hago, no se oculte. así más explicativo tambien 
    [HttpGet("deceased/{deceasedId}")]
    public async Task<ActionResult<List<MemoryResponseDTO>>> GetByDeceased(int deceasedId, [FromQuery] bool approved = true) //     Permite añadir ?approved=false en la URL. //el deceased lo coge del nombre de la dirección directamente
    {
        try
        {
            var list = await _memoryService.GetByDeceasedIdAsync(deceasedId, approved);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error en Controller al obtener memorias del difunto {deceasedId}");
            return StatusCode(500, "Ocurrió un error interno al recuperar los datos.");
        }
    }


    [HttpPost]
    [Authorize(Roles = Roles.StandardUser)]//extraigo el id de usuario del token de user --> se asigna a l objeto antes de llamar al service
    public async Task<ActionResult> AddMemory([FromBody] MemoryCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Fallo al validar memoria debido a un formato de datos enviados inválido.");
            return BadRequest(ModelState);
        }
        try
        {

            //cojo id de los claims --> el objeto User es una propiedad de ControllerBase que representa al usuario autenticado que hace la petición, por eso tengo acceso a él.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null) return Unauthorized(); //problema al obtener claim --> problema con el token

            //como el vlaor de claim llega como string xq JSON, hay que parsearlo a string
            //no puedo hacer cast directo (int) porque no son tipos compatibles
            int currentUserId = int.Parse(userIdClaim.Value);



            var newId = await _memoryService.AddMemoryAsync(dto, currentUserId);
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


    //para utilizar varios hay ue concatenar strings
    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Guardian)]
    //TODO: COMPROBAR QUE el Guardián es EL guardián de ESTE difunto.
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
            bool hasBeenUpdated = await _memoryService.UpdateStatusAsync(id, status);

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
    // LO PONGO COMO AUTHORIZED, ABIERTO A LOS LOGGEADOS, comprobación dentro xq son todos con condiciones
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            //recupero la memoria para tener los datos para filtrado
            var memoryDTO = await _memoryService.GetByIdAsync(id);

            //cojo los datos del usuario peticionario
            var currentlyUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (memoryDTO == null)
            {
                _logger.LogWarning($"Intento de eliminación de memoria con id {id} que no existe.");
                return NotFound();
            }

            //permisos: admin? --> su autor? ---> su guardian?
            bool canDelete = false;

            //admin
            if (User.IsInRole(Roles.Admin)) canDelete = true;
            //author
            else if (memoryDTO.UserId == currentlyUserId) canDelete = true;
            //guardian
            else if (User.IsInRole(Roles.Guardian))
            {

                //traigo sus difuntos a cargo 
                var myDeceasedList = await _deceasedService.GetByGuardianIdAsync(currentlyUserId);

                if(myDeceasedList != null)
            {
               
                foreach (var deceased in myDeceasedList)
                {
                    if (deceased.Id == memoryDTO.DeceasedId)
                    {
                        canDelete = true;
                        break;
                    }
                } 
            }


            }

            if (canDelete)
            {
                var hasBeenDeleted = await _memoryService.DeleteMemoryAsync(id);

                if (!hasBeenDeleted)
                {
                    _logger.LogWarning($"Fracaso al eliminar memoria con id {id}, no encontrado en base de datos");
                    return NotFound();
                }
                return NoContent();
            }

            return Forbid();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al procesar delete memory en controller");
            return StatusCode(500, "Error interno al procesar petición");
        }
    }

}