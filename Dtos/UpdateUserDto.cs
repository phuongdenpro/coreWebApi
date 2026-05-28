using coreWebApi.Models;
using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }


    [Required(ErrorMessage = "Full name is required")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;


    public Gender Gender { get; set; } = Gender.Other;

    public string? Password { get; set; }
    public string? Role { get; set; }
}