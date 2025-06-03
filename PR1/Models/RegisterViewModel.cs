using System.ComponentModel.DataAnnotations;

namespace PR1.Models;


public class RegisterViewModel
{
    [Required(ErrorMessage = "Обязательное поле")]
    [StringLength(20, ErrorMessage = "Максимум 20 символов")]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Обязательное поле")]
    [StringLength(50, ErrorMessage = "Пароль должен быть от {2} до {1} символов", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    [RegularExpression(@"^(?=.*[A-Za-zА-Яа-я])(?=.*\d)(?=.*[!@#$%^&*]).+$", 
        ErrorMessage = "Пароль должен содержать буквы, цифры и спецсимволы")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтвердите пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }
}