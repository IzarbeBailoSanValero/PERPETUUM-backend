using System.ComponentModel.DataAnnotations;

namespace PERPETUUM.DTOs;

public class UserDtoIn
    {
        [Required]
        public string UserName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        
        [Required]
        [StringLength(15, ErrorMessage = "Password must be 15 characters")]
        public string Password { get; set; }
}

