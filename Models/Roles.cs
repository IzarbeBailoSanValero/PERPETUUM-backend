using System.ComponentModel.DataAnnotations;
namespace PERPETUUM.Models;
public static class Roles
{
    public const string Admin = "Admin";       //superusuario
    public const string Staff = "Staff";       //trabajador de funeraria
    public const string Guardian = "Guardian"; //quien administra el perfil del difunto
    public const string StandardUser = "StandardUser"; //quien public acondolencias
}

