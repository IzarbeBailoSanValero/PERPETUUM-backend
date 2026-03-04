using Microsoft.AspNetCore.Mvc;
using PERPETUUM.DTOs;
using PERPETUUM.Models; //para utilizar los roles en la protección
using PERPETUUM.Services;
using Microsoft.AspNetCore.Authorization;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using System.Security.Claims;
namespace PERPETUUM.Controllers;


[Route("api/[controller]")]
[ApiController]
//hago la proteccion a nivel de función
public class MemoryController : ControllerBase
{
    private readonly IMemoryService _memoryService;
    private readonly ILogger<MemoryController> _logger;
    private readonly IDeceasedService _deceasedService;
    private readonly Cloudinary _cloudinary;

    public MemoryController(
        IMemoryService service,
        ILogger<MemoryController> logger,
        IMemorialGuardianService guardianService,
        IDeceasedService deceasedService,
        Cloudinary cloudinary)
    {
        _memoryService = service;
        _logger = logger;
        _deceasedService = deceasedService;
        _cloudinary = cloudinary;
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
    [Authorize(Roles = Roles.StandardUser + "," + Roles.Guardian)]//extraigo el id de usuario del token --> se asigna a l objeto antes de llamar al service
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

    [HttpPost("photo")]
    [Authorize(Roles = Roles.StandardUser + "," + Roles.Guardian)]
    [RequestSizeLimit(15_000_000)] // 15MB
    public async Task<ActionResult> AddMemoryWithPhoto([FromForm] MemoryPhotoCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Fallo al validar memoria (foto) por datos inválidos.");
            return BadRequest(ModelState);
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int currentUserId = int.Parse(userIdClaim.Value);

            if (dto.Photo == null || dto.Photo.Length == 0)
            {
                return BadRequest("La imagen es obligatoria.");
            }

            if (dto.Type != 3)
            {
                return BadRequest("Este endpoint es solo para recuerdos con foto (Type=3).");
            }

            if (dto.Photo.ContentType == null || !dto.Photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("El archivo debe ser una imagen.");
            }

            await using var stream = dto.Photo.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(dto.Photo.FileName, stream),
                Folder = $"perpetuum/memories/deceased-{dto.DeceasedId}",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary error: {Message}", uploadResult.Error.Message);
                return StatusCode(502, "Error subiendo la imagen.");
            }
            var secureUrl = uploadResult.SecureUrl?.ToString();
            if (string.IsNullOrWhiteSpace(secureUrl))
            {
                _logger.LogError("Cloudinary upload sin SecureUrl para deceased {DeceasedId}", dto.DeceasedId);
                return StatusCode(502, "Error subiendo la imagen.");
            }

            var memoryDto = new MemoryCreateDTO
            {
                DeceasedId = dto.DeceasedId,
                Type = 3,
                TextContent = dto.TextContent,
                MediaURL = secureUrl,
                AuthorRelation = dto.AuthorRelation
            };

            var newId = await _memoryService.AddMemoryAsync(memoryDto, currentUserId);
            return CreatedAtAction(nameof(GetByDeceased), new { deceasedId = dto.DeceasedId }, new { id = newId, mediaUrl = memoryDto.MediaURL });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Reglas de negocio (memory photo): {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al crear memoria con foto.");
            return StatusCode(500, "Error interno al procesar petición");
        }
    }


    //para utilizar varios hay ue concatenar strings
    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Guardian)]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] int statusInt)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Fallo al validar memoria debido a un formato de datos enviados inválido.");
            return BadRequest(ModelState);
        }

        try
        {
            //recupero la memoria para tener los datos para filtrado
            var memoryDTO = await _memoryService.GetByIdAsync(id);

            if (memoryDTO == null)
            {
                return NotFound($"No se encontró la memoria con ID {id}.");
            }

            //cojo los datos del usuario peticionario
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var currentlyUserId = int.Parse(userIdClaim.Value);

            //permisos: admin?---> su guardian?
            bool canUpdate = false;



            //admin
            if (User.IsInRole(Roles.Admin))
            {
                canUpdate = true;
            }
            //guardian
            else if (User.IsInRole(Roles.Guardian))
            {

                //traigo sus difuntos a cargo 
                var myDeceasedList = await _deceasedService.GetByGuardianIdAsync(currentlyUserId);

                if (myDeceasedList != null)
                {

                    foreach (var deceased in myDeceasedList)
                    {
                        if (deceased.Id == memoryDTO.DeceasedId)
                        {
                            canUpdate = true;
                            break;
                        }
                    }
                }

          }

                if (!canUpdate)
                {
                    return Forbid();
                }

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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var currentlyUserId = int.Parse(userIdClaim.Value);

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

                if (myDeceasedList != null)
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

            if (!canDelete)
            {
                return Forbid();
            }

            var hasBeenDeleted = await _memoryService.DeleteMemoryAsync(id);

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