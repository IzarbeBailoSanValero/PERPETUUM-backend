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


    }
}