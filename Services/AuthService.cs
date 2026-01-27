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




namespace PERPETUUM.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _repository;

        public AuthService(IConfiguration configuration, IUserRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        //DAME UN USUARIO CONE STAS CREDENCIALES: envía el Dto de los credenciales del usuario, comprueba qué usuario es y devuelve el token jwt
        public string Login(LoginDtoIn loginDtoIn)
        {
            // 1. El repositorio busca y valida password
            var userDto = _repository.GetUserFromCredentials(loginDtoIn);

            // 2.
            if (userDto == null)
            {
                // Lanzamos la excepción que tu Controller está esperando capturar
                throw new KeyNotFoundException("Usuario o contraseña incorrectos.");
            }

            // 3. Si existe, generamos token
            return GenerateToken(userDto);
        }

        //CREA UN USUARIO CON ESTAS CREDENCIALES
        public string Register(UserDtoIn userDtoIn)
        {
            var user = _repository.AddUserFromCredentials(userDtoIn);
            return GenerateToken(user);
        }

        //CONFIGURACIONES DE LA GENERACIÓN DEDtoKEN
        public string GenerateToken(UserDtoOut userDtoOut)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Convert.ToString(userDtoOut.UserId)),
                        new Claim(ClaimTypes.Name, userDtoOut.UserName),
                        new Claim(ClaimTypes.Role, userDtoOut.Role),
                        new Claim(ClaimTypes.Email, userDtoOut.Email)
                    }),
                Expires = DateTime.UtcNow.AddDays(7), // AddMinutes(60)
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
