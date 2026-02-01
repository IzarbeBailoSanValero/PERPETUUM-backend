using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;



public class UserDtoOut
{
    public int Id { get; set; }        
        public string Name { get; set; } = string.Empty;   
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
}