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
        public string Login(LoginDtoIn loginDtoIn) {
            var user = _repository.GetUserFromCredentials(loginDtoIn);
            return GenerateToken(user);
        }

        //CREA UN USUARIO CON ESTAS CREDENCIALES
        public string Register(UserDtoIn userDtoIn) {
            var user = _repository.AddUserFromCredentials(userDtoIn);
            return GenerateToken(user);
        }

        //CONFIGURACIONES DE LA GENERACIÓN DEDtoKEN
        public string GenerateToken(UserDtoOut userDtoOut) {
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
                        new Claim(ClaimTypes.Email, userDtoOut.Email),
                        new Claim("myCustomClaim", "myCustomClaimValue"),
                        // add other claims
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
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier); //filtrado
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId)) 
            { 
                return false; 
            }
            var isOwnResource = userId == requestedUserID;

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim != null) return false; 
            var isAdmin = roleClaim!.Value == Roles.Admin;
            
            var hasAccess = isOwnResource || isAdmin;
            return hasAccess;
        }
     

    }
}
