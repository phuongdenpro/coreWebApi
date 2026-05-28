using coreWebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace coreWebApi.Dtos
{
    public class AdminRegisterDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;


        public Gender Gender { get; set; } = Gender.Other;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

    }
}
