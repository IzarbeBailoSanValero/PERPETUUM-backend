using Microsoft.AspNetCore.Mvc;
using PERPETUUM.Services;
using PERPETUUM.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace PERPETUUM.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")] 
    public class DeceasedController : ControllerBase
    {
        private readonly IDeceasedService _deceasedService;
        private readonly ILogger<DeceasedController> _logger;

        public DeceasedController(IDeceasedService deceasedService, ILogger<DeceasedController> logger)
        {
            _deceasedService = deceasedService;
            _logger = logger;
        }

        
        [HttpGet]
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
        public async Task<ActionResult<List<DeceasedResponseDTO>>> Search([FromQuery] DeceasedSearchDTO searchDTO)
        {
            try
            {
                List<DeceasedResponseDTO> result = await _deceasedService.SearchDeceasedAsync(searchDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al buscar difuntos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

       
        [HttpPost]
        public async Task<ActionResult<DeceasedResponseDTO>> CreateDeceased([FromBody] DeceasedCreateDTO deceasedDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Fallo al validar difunto debido a un formato de datos enviados inválido.");
                return BadRequest(ModelState);
            }

            try
            {
            
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
        public async Task<IActionResult> UpdateDeceased(int deceasedId, [FromBody] DeceasedUpdateDTO deceasedDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Fallo al validar difunto debido a un formato de datos enviados inválido.");
                return BadRequest(ModelState);
            }

            if (deceasedId != deceasedDto.Id)
            {
                _logger.LogWarning("No coincide el id del objeto a actualizar y el de la petición.");
                return BadRequest("No coincide el id del objeto a actualizar y el de la petición.");
            }

            try
            {
                bool hasBeenUpdated = await _deceasedService.UpdateDeceasedAsync(deceasedDto);

                if (hasBeenUpdated)
                {
                    _logger.LogInformation($"Éxito al actualizar el difunto con id {deceasedId}");
                    return NoContent(); // 204
                }
                else
                {
                    _logger.LogWarning($"Fracaso al actualizar el difunto con id {deceasedId}, no encontrado en base de datos");
                    return NotFound($"No encontrado difunto con ID {deceasedId}. Fallo al actualizar");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Error respecto a las reglas de negocio en update");
                return Conflict(ex.Message); //409
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar update deceased en controller");
                return StatusCode(500, "Error interno al procesar petición");
            }
        }

        [HttpDelete("{deceasedId}")]
        public async Task<IActionResult> DeleteDeceased(int deceasedId)
        {
            try 
            {
                var hasBeenDeleted = await _deceasedService.DeleteDeceasedAsync(deceasedId);

                if (!hasBeenDeleted)
                {
                    _logger.LogWarning($"Fracaso al eliminar el difunto con id {deceasedId}, no encontrado en base de datos");
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

//TODO: SI DA TIEMPO: EL TRABAJADOR DEBERÍA SACAR EL ID DE SU FUNERARIA PARA AL CREACIÓN DEL DIFUNTO Y MEMORIALGUARDIAN A TRAVES DE SU JWTOKEN. COMPROBAR QUE ESTÁ ASÍ O SE PUEDE