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
    private readonly PERPETUUM.Services.CloudinaryWrapper _cloudinaryWrapper;

    public MemoryController(
        IMemoryService service,
        ILogger<MemoryController> logger,
        IMemorialGuardianService guardianService,
        IDeceasedService deceasedService,
        PERPETUUM.Services.CloudinaryWrapper cloudinaryWrapper)
    {
        _memoryService = service;
        _logger = logger;
        _deceasedService = deceasedService;
        _cloudinaryWrapper = cloudinaryWrapper;
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

    [HttpGet("pending")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Guardian)]
    public async Task<ActionResult<List<MemoryResponseDTO>>> GetPendingMemories()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                return Unauthorized();

            List<int>? deceasedIds = null;
            if (User.IsInRole(Roles.Guardian))
            {
                var myDeceased = await _deceasedService.GetByGuardianIdAsync(currentUserId);
                deceasedIds = myDeceased?.Select(d => d.Id).ToList() ?? new List<int>();
            }

            var list = await _memoryService.GetPendingMemoriesAsync(deceasedIds);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener memorias pendientes");
            return StatusCode(500, "Error interno al recuperar los datos.");
        }
    }

    /// <summary>UserId para guardar en Memory cuando el autor es Guardian (Memory.UserId FK a User; Guardian no tiene User.Id).</summary>
    private const int GuardianAuthorUserId = 1;

    [HttpPost]
    [Authorize(Roles = Roles.StandardUser + "," + Roles.Guardian)]
    public async Task<ActionResult> AddMemory([FromBody] MemoryCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Fallo al validar memoria debido a un formato de datos enviados inválido.");
            return BadRequest(ModelState);
        }
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int claimUserId))
            {
                _logger.LogWarning("Token sin identificador de usuario válido.");
                return Unauthorized();
            }
            // Guardian no tiene User.Id; usamos un UserId de sistema para cumplir la FK.
            int userIdForMemory = User.IsInRole(Roles.Guardian) ? GuardianAuthorUserId : claimUserId;

            var newId = await _memoryService.AddMemoryAsync(dto, userIdForMemory);
            return CreatedAtAction(nameof(GetByDeceased), new { deceasedId = dto.DeceasedId }, new { id = newId });



        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error respecto a las reglas de negocio en memory");
            return Conflict(ex.Message);
        }
        catch (MySqlConnector.MySqlException ex)
        {
            _logger.LogError(ex, "Error MySQL en AddMemory");
            return BadRequest("No se pudo guardar el recuerdo. Comprueba que el difunto existe y que estás logueado como usuario (no como staff/guardian).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al procesar create memory en controller");
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
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int claimUserId))
            {
                _logger.LogWarning("Token sin identificador de usuario válido (photo).");
                return Unauthorized();
            }
            int userIdForMemory = User.IsInRole(Roles.Guardian) ? GuardianAuthorUserId : claimUserId;

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

            if (!_cloudinaryWrapper.IsConfigured || _cloudinaryWrapper.Instance == null)
            {
                return StatusCode(503, "Subida de fotos no disponible. Configure la variable CLOUDINARY_URL (formato: cloudinary://api_key:api_secret@cloud_name) en el servidor.");
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

            var uploadResult = await _cloudinaryWrapper.Instance.UploadAsync(uploadParams);
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
            var mediaUrl1x1 = PERPETUUM.Services.CloudinaryUrlHelper.To1x1Url(secureUrl);

            var memoryDto = new MemoryCreateDTO
            {
                DeceasedId = dto.DeceasedId,
                Type = 3,
                TextContent = dto.TextContent,
                MediaURL = mediaUrl1x1,
                AuthorRelation = dto.AuthorRelation
            };

            var newId = await _memoryService.AddMemoryAsync(memoryDto, userIdForMemory);
            return CreatedAtAction(nameof(GetByDeceased), new { deceasedId = dto.DeceasedId }, new { id = newId, mediaUrl = memoryDto.MediaURL });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Reglas de negocio (memory photo): {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (MySqlConnector.MySqlException ex)
        {
            _logger.LogError(ex, "Error MySQL en AddMemoryWithPhoto");
            return BadRequest("No se pudo guardar el recuerdo. Comprueba que el difunto existe y que estás logueado como usuario (no como staff/guardian).");
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
    public async Task<ActionResult> UpdateStatus(int id, [FromQuery] int status)
    {
        try
        {
            var memoryDTO = await _memoryService.GetByIdAsync(id);

            if (memoryDTO == null)
            {
                return NotFound($"No se encontró la memoria con ID {id}.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out int currentlyUserId))
                return Unauthorized();

            bool canUpdate = false;

            if (User.IsInRole(Roles.Admin))
            {
                canUpdate = true;
            }
            else if (User.IsInRole(Roles.Guardian))
            {
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

            var statusEnum = (MemoryStatus)status;

                bool hasBeenUpdated = await _memoryService.UpdateStatusAsync(id, statusEnum);

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
    [Authorize] // Requiere sesión activa; los permisos finos (autor / guardian / admin) se comprueban dentro
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