//cosas de jwt
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

//otros
using PERPETUUM.Repositories;
using PERPETUUM.DTOs;
using PERPETUUM.Models;
using Microsoft.Extensions.Logging;

//hasheado
using BCrypt.Net;




namespace PERPETUUM.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        //repositorios para login (forma de integrar el login en los tres perfiles sugerida IA)
        private readonly IStaffRepository _staffRepo;
        private readonly IMemorialGuardianRepository _guardianRepo;
        private readonly IUserRepository _userRepo;

        public AuthService(
            IConfiguration configuration,
            IStaffRepository staffRepo,
            IMemorialGuardianRepository guardianRepo,
            IUserRepository userRepo,
            ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _staffRepo = staffRepo;
            _guardianRepo = guardianRepo;
            _userRepo = userRepo;
            _logger = logger;
        }



        public async Task<string> LoginAsync(LoginDtoIn loginDto)
        {
            var staff = await _staffRepo.GetByEmailAsync(loginDto.Email);
            if (staff != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.PasswordHash))
                {
                    string role = staff.IsAdmin ? Roles.Admin : Roles.Staff;
                    return GenerateToken(staff.Id, staff.Name, staff.Email, role, staff.FuneralHomeId);
                }
            }

            var guardian = await _guardianRepo.GetByEmailAsync(loginDto.Email);
            if (guardian != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, guardian.PasswordHash))
                {
                    return GenerateToken(guardian.Id, guardian.Name, guardian.Email, Roles.Guardian, guardian.FuneralHomeId);
                }
            }

            var user = await _userRepo.GetByEmailAsync(loginDto.Email);
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return GenerateToken(user.Id, user.Name, user.Email, Roles.StandardUser, null);
                }
            }

            _logger.LogWarning($"Login fallido asíncrono: {loginDto.Email}");
            throw new KeyNotFoundException("Usuario o contraseña incorrectos.");
        }


        public async Task<string> RegisterAsync(UserDtoIn userDtoIn)
        {
            var existing = await _userRepo.GetByEmailAsync(userDtoIn.Email);
            if (existing != null)
            {
                throw new ArgumentException("El email ya está registrado.");
            }


            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDtoIn.Password);

            var newUser = new User
            {
                Name = userDtoIn.Name,
                Email = userDtoIn.Email,
                PasswordHash = passwordHash
            };

            int newId = await _userRepo.CreateUserAsync(newUser);

            return GenerateToken(newId, newUser.Name, newUser.Email, Roles.StandardUser, null);
        }






        //CONFIGURACIONES DE LA GENERACIÓN DEDtoKEN
        //modifico la definición del método de Alejandro de  GenerateToken para que acepte los 5 parámetros individuales para que acepte staff, guardian y user, en lugar de usar un mismo dto (idea IA)

        private string GenerateToken(int userId, string userName, string email, string role, int? funeralHomeId)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]);
            
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Email, email)
            };

            // Solo añadimos el FuneralHomeId si tiene valor --> staff y guardian. user no.
            if (funeralHomeId.HasValue)
            {
                claims.Add(new Claim("FuneralHomeId", funeralHomeId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token válido por 7 días
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }




        //metodo creado por alejandro: 
        // ClaimsPrincipal es una clase que crea el porpio .net (un IEnumerable, un list) para que cuando recibe un token, lo descifra y guarda ahí las propiedades
        //para coger el rol: user.Claims.FirstOrDefault(condición) FirstOrDefault es un método LINQ que:  Devuelve el primer elemento que cumpla una condición
        public bool HasAccessToResource(int requestedUserID, ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return false;
            }

            // 1. ¿Es el propio dueño del recurso?
            var isOwnResource = (userId == requestedUserID);

            // 2. ¿Es Admin?
            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            // Si roleClaim es nulo, no es admin. Si tiene valor, comprobamos si es Admin.
            var isAdmin = roleClaim != null && roleClaim.Value == Roles.Admin;

            // Accede si es suyo O SI es admin
            return isOwnResource || isAdmin;
        }


    }
}
