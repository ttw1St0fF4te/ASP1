namespace PR1.Models;

using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "Обязательное поле")]
    [StringLength(20, ErrorMessage = "Максимум 20 символов")]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Обязательное поле")]
    [StringLength(50, ErrorMessage = "Максимум 50 символов")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}