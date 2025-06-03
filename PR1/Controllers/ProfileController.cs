using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PR1.Data;
using PR1.Models;
using PR1.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PR1.Controllers
{
    [Authorize(Roles = "user")]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ProfileController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactData(string username, string email)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            // Валидация имени пользователя
            if (string.IsNullOrWhiteSpace(username) || username.Length > 20 || !IsValidUsername(username))
            {
                TempData["Error"] = "Имя пользователя должно содержать только буквы и цифры, максимум 20 символов.";
                return RedirectToAction("Index");
            }

            // Валидация email
            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                TempData["Error"] = "Некорректный формат email адреса.";
                return RedirectToAction("Index");
            }

            user.Username = username;
            user.Email = email;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Контактные данные успешно обновлены.";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            var orders = await _context.Orders
                .Where(o => o.UserId == int.Parse(userId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Передаем информацию о доступности кошелька
            ViewBag.HasWallet = (user?.TotalSpent ?? 0) >= 30000;

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> LoyaltyProgram()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            var totalSpent = user.TotalSpent ?? 0;
            var currentLevel = LoyaltyLevels.GetLoyaltyLevel(totalSpent);
            var cashbackPercent = LoyaltyLevels.GetCashbackPercent(currentLevel);

            var viewModel = new LoyaltyProgramViewModel
            {
                CurrentLevel = currentLevel ?? "Нет уровня",
                CashbackPercent = cashbackPercent * 100,
                TotalSpent = totalSpent,
                ProgressToNext = CalculateProgressToNext(totalSpent)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Wallet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            var totalSpent = user.TotalSpent ?? 0;
            var currentLevel = LoyaltyLevels.GetLoyaltyLevel(totalSpent);

            // Кошелек доступен только с базового уровня
            if (currentLevel == null)
            {
                return RedirectToAction("LoyaltyProgram");
            }

            var walletBalance = user.WalletBalance ?? 0;

            return View(new WalletViewModel
            {
                Balance = walletBalance,
                CurrentLevel = currentLevel
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                TempData["Error"] = "Для смены пароля необходимо указать email в контактных данных.";
                return RedirectToAction("Index");
            }

            // Генерируем новый пароль
            var newPassword = GenerateSecurePassword();
            user.Password = newPassword;

            await _context.SaveChangesAsync();

            // Отправляем пароль на email
            await SendPasswordResetEmail(user.Email, newPassword);

            // Разлогиниваем пользователя
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] = "Новый пароль отправлен на ваш email. Используйте его для входа в систему.";
            return RedirectToAction("Login", "Account");
        }

        private bool IsValidUsername(string username)
        {
            return username.All(c => char.IsLetterOrDigit(c));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string CalculateProgressToNext(decimal totalSpent)
        {
            if (totalSpent < LoyaltyLevels.Levels[LoyaltyLevels.Basic].MinSpent)
            {
                var needed = LoyaltyLevels.Levels[LoyaltyLevels.Basic].MinSpent - totalSpent;
                return $"До Базового уровня осталось потратить {needed:F0} ₽";
            }
            if (totalSpent < LoyaltyLevels.Levels[LoyaltyLevels.Silver].MinSpent)
            {
                var needed = LoyaltyLevels.Levels[LoyaltyLevels.Silver].MinSpent - totalSpent;
                return $"До Серебряного уровня осталось потратить {needed:F0} ₽";
            }
            if (totalSpent < LoyaltyLevels.Levels[LoyaltyLevels.Gold].MinSpent)
            {
                var needed = LoyaltyLevels.Levels[LoyaltyLevels.Gold].MinSpent - totalSpent;
                return $"До Золотого уровня осталось потратить {needed:F0} ₽";
            }
            return "Достигнут максимальный уровень";
        }

        private string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new StringBuilder();

            // Обеспечиваем наличие хотя бы одного символа каждого типа
            password.Append(chars[random.Next(0, 26)]); // Заглавная буква
            password.Append(chars[random.Next(26, 52)]); // Строчная буква
            password.Append(chars[random.Next(52, 62)]); // Цифра
            password.Append(chars[random.Next(62, chars.Length)]); // Специальный символ

            // Добавляем еще 4 случайных символа
            for (int i = 0; i < 4; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        private async Task SendPasswordResetEmail(string email, string newPassword)
        {
            var emailBody = GeneratePasswordResetEmailBody(newPassword);
            
            // Используем существующий EmailService с модификацией
            await _emailService.SendPasswordResetAsync(email, emailBody);
        }

        private string GeneratePasswordResetEmailBody(string newPassword)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            
            // Заголовок
            sb.AppendLine("<div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; margin-bottom: 20px;'>");
            sb.AppendLine("<h1 style='color: #007bff; margin: 0;'>MoeShop</h1>");
            sb.AppendLine("<h2 style='color: #dc3545; margin: 10px 0 0 0;'>Смена пароля</h2>");
            sb.AppendLine("</div>");
            
            // Основная информация
            sb.AppendLine("<div style='background-color: #fff; border: 1px solid #dee2e6; border-radius: 8px; padding: 20px; margin-bottom: 20px;'>");
            sb.AppendLine("<p>Вы запросили смену пароля для вашего аккаунта в MoeShop.</p>");
            sb.AppendLine($"<p><strong>Ваш новый пароль:</strong> <span style='background-color: #f8f9fa; padding: 5px 10px; border-radius: 4px; font-family: monospace; font-size: 16px;'>{newPassword}</span></p>");
            sb.AppendLine("<p style='color: #dc3545;'><strong>Важно:</strong> Сохраните этот пароль в безопасном месте.</p>");
            sb.AppendLine("</div>");
            
            // Футер
            sb.AppendLine("<div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; color: #6c757d;'>");
            sb.AppendLine("<p style='margin: 5px 0;'>С уважением, команда MoeShop</p>");
            sb.AppendLine("<p style='margin: 5px 0; font-size: 12px;'>Это автоматическое сообщение, не отвечайте на него.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");
            
            return sb.ToString();
        }
    }

    // ViewModels
    public class LoyaltyProgramViewModel
    {
        public string CurrentLevel { get; set; }
        public decimal CashbackPercent { get; set; }
        public decimal TotalSpent { get; set; }
        public string ProgressToNext { get; set; }
    }

    public class WalletViewModel
    {
        public decimal Balance { get; set; }
        public string CurrentLevel { get; set; }
    }
}