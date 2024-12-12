namespace PR1.Models;

using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}