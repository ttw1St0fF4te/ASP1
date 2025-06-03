using System.ComponentModel.DataAnnotations;

namespace PR1.Models;

public class CheckoutViewModel
{
    public Cart Cart { get; set; }
    public decimal TotalAmount { get; set; }
    public string UserEmail { get; set; }
    
    // Контактные данные
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string CustomerName { get; set; }
    
    [Required(ErrorMessage = "Телефон обязателен для заполнения")]
    [RegularExpression(@"^\+7 \([0-9]{3}\) [0-9]{3}-[0-9]{2}-[0-9]{2}$", 
        ErrorMessage = "Телефон должен быть в формате +7 (999) 999-99-99")]
    public string CustomerPhone { get; set; }
    
    // Адрес доставки
    [Required(ErrorMessage = "Страна обязательна для заполнения")]
    [StringLength(50, ErrorMessage = "Название страны не должно превышать 50 символов")]
    public string Country { get; set; }
    
    [Required(ErrorMessage = "Город обязателен для заполнения")]
    [StringLength(50, ErrorMessage = "Название города не должно превышать 50 символов")]
    public string City { get; set; }
    
    [Required(ErrorMessage = "Индекс обязателен для заполнения")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Индекс должен состоять из 6 цифр")]
    public string PostalCode { get; set; }
    
    [Required(ErrorMessage = "Улица обязательна для заполнения")]
    [StringLength(100, ErrorMessage = "Название улицы не должно превышать 100 символов")]
    public string Street { get; set; }
    
    [Required(ErrorMessage = "Номер дома обязателен для заполнения")]
    [StringLength(20, ErrorMessage = "Номер дома не должен превышать 20 символов")]
    public string HouseNumber { get; set; }
    
    // Информация о лояльности и кошельке
    public string LoyaltyLevel { get; set; }
    public decimal? WalletBalance { get; set; }
    public bool CanUseWallet { get; set; }
    public bool UseWallet { get; set; } // true - списать, false - копить
    public decimal? PotentialEarnings { get; set; } // Сколько можно заработать
    
    // Итоговые расчеты
    public decimal WalletDeduction { get; set; }
    public decimal FinalAmount { get; set; }
}