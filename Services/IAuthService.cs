using System.Security.Claims;
using PERPETUUM.DTOs;




namespace PERPETUUM.Services
{
    public interface IAuthService
    {
        //las transformo en asincronas porque van a base de datos las dos primeras
       Task<string> LoginAsync(LoginDtoIn loginDto);
        Task<string> RegisterAsync(UserDtoIn userDto);
        public bool HasAccessToResource(int requestedUserID, ClaimsPrincipal user); //es la manera que utiliza alejandro que sirve para comprobar si con la información que hay ene l token, al descifrala, el usuario puede acceder a esos recursos , bien si son suyos o si es admin
        


    }
}
