using System.Security.Claims;
using PERPETUUM.DTOs;




namespace PERPETUUM.Services
{
    public interface IAuthService
    {
        public string Login(LoginDtoIn userDtoIn);
        public string Register(UserDtoIn userDtoIn);
        public string GenerateToken(UserDtoOut userDtoOut);
        public bool HasAccessToResource(int requestedUserID, ClaimsPrincipal user); //es la manera que utiliza alejandro que sirve para comprobar si con la información que hay ene l token, al descifrala, el usuario puede acceder a esos recursos , bien si son suyos o si es admin
        


    }
}
