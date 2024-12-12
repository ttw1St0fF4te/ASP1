using System.ComponentModel.DataAnnotations;

namespace PR1.Models;


public class RegisterViewModel
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтвердите пароль")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}