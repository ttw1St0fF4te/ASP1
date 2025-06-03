using System.ComponentModel.DataAnnotations;

namespace PR1.Models;

public class ReviewViewModel
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Выберите оценку")]
    [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
    [Display(Name = "Оценка")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Введите текст отзыва")]
    [MinLength(10, ErrorMessage = "Минимум 10 символов")]
    [Display(Name = "Текст отзыва")]
    public string Text { get; set; }
}