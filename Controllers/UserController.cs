using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PERPETUUM.DTOs;
using PERPETUUM.Services;


namespace PERPETUUM.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] //debe estar loggeado para cualquier endpoint
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService; //lo llamo para comprobar si hay acceso al recurso
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, IAuthService authService, ILogger<UserController> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }



    [HttpGet("{id}")]
    public async Task<ActionResult<UserDtoOut>> GetProfile(int id)
    {
        try
        {

            if (!_authService.HasAccessToResource(id, User)) return Forbid(); //error 403 --> Forbidden  : diferencia con 401 unautorized --> Un error 401 ocurre cuando hay un intento de acceso "no autorizado" en el servidor. un error 403 Forbidden ocurre cuando el servidor reconoce al usuario pero determina que no tiene los permisos necesarios.

            var user = await _userService.GetUserProfileAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {

            _logger.LogWarning("Error respecto a las reglas de negocio en create");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al procesar create user en controller");
            return StatusCode(500, "Error interno al procesar petición");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProfile(int id, [FromBody] UserUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_authService.HasAccessToResource(id, User)) return Forbid(); //error 403 --> Forbidden  : diferencia con 401 unautorized --> Un error 401 ocurre cuando hay un intento de acceso "no autorizado" en el servidor. un error 403 Forbidden ocurre cuando el servidor reconoce al usuario pero determina que no tiene los permisos necesarios.

            var updated = await _userService.UpdateUserProfileAsync(id, dto);

            if (!updated)
            {
                return NotFound(new { message = "No se pudo actualizar. El usuario no existe." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {

            _logger.LogWarning("Error respecto a las reglas de negocio en create");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al procesar update user en controller");
            return StatusCode(500, "Error interno al procesar petición");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAccount(int id)
    {
        try
        {
            if (!_authService.HasAccessToResource(id, User)) return Forbid(); //error 403 --> Forbidden  : diferencia con 401 unautorized --> Un error 401 ocurre cuando hay un intento de acceso "no autorizado" en el servidor. un error 403 Forbidden ocurre cuando el servidor reconoce al usuario pero determina que no tiene los permisos necesarios.

            var deleted = await _userService.DeleteUserAccountAsync(id);

            if (!deleted) 
            {
                return NotFound(new { message = "El usuario ya no existe o no se pudo borrar." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico eliminando cuenta {Id}", id);
            return StatusCode(500, new { message = "Error interno eliminando cuenta", details = ex.Message });
        }
    }
}