using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class LoginDtoIn
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe ser de al menos 6 caracteres")]
        public string Password { get; set; }
}

