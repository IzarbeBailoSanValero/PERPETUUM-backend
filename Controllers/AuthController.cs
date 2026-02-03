using Microsoft.AspNetCore.Mvc;
using PERPETUUM.DTOs;
using PERPETUUM.Services;
using Microsoft.AspNetCore.Authorization;

namespace PERPETUUM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDtoIn loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var token = await _authService.LoginAsync(loginDto);
                return Ok(new { token });
            }
            catch (KeyNotFoundException)
            {
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detail = ex.Message });
            }
        }



        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDtoIn userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var token = await _authService.RegisterAsync(userDto);
                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en registro", detail = ex.Message });
            }
        }

        // //para poder generar un hash y crear un administrador para loggearme con él (problema bcrypt) // busco la solución
        // [HttpGet("test-hash")]
        // public IActionResult GetHash()
        // {
        //     return Ok(BCrypt.Net.BCrypt.HashPassword("admin123"));
        // }

        // //resultado -->  $2a$11$jNRYr2iZD2xWNHtuXD6Gj.S4kU6OsQd/B/G49eo49pbuzOORQ1oh6


    }
}