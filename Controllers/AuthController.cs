using Microsoft.AspNetCore.Mvc;
using PERPETUUM.DTOs;
using PERPETUUM.Services;
using Microsoft.AspNetCore.Authorization;

namespace PERPETUUM.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Recomendado: api/auth en vez de solo /auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginDtoIn loginDtoIn)
        {
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }

                var token = _authService.Login(loginDtoIn);
                
                // MEJORA IA: Devolver JSON en lugar de string plano
                return Ok(new { token = token }); 
            }
            catch (KeyNotFoundException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating the token", details = ex.Message });
            }
        }

        [HttpPost("Register")]
        public IActionResult Register(UserDtoIn userDtoIn)
        {
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }

                var token = _authService.Register(userDtoIn);
                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error registering user", details = ex.Message });
            }
        }
    }
}