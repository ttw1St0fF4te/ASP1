using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PR1.Data;
using PR1.Models;
using PR1.Services;

namespace PR1.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AccountController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.UserRole.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Add user ID claim
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }
        return View(model);
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                Username = model.Username,
                Password = model.Password, // Store the password as plain text for simplicity
                UserRoleId = 3 // Id роли "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "user"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Add user ID claim
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }
    
    // GET: /Account/ForgotPassword
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: /Account/ForgotPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Введите email адрес.");
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            ModelState.AddModelError("", "Пользователь с таким email не найден.");
            return View();
        }

        // Генерируем новый пароль (тот же метод что и в ProfileController)
        var newPassword = GenerateSecurePassword();
        user.Password = newPassword;

        await _context.SaveChangesAsync();

        // Отправляем пароль на email
        await SendPasswordResetEmail(email, newPassword);

        TempData["Success"] = "Новый пароль отправлен на ваш email.";
        return RedirectToAction("Login");
    }

    // Добавьте эти приватные методы в AccountController:
    private string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new StringBuilder();

        password.Append(chars[random.Next(0, 26)]); // Заглавная буква
        password.Append(chars[random.Next(26, 52)]); // Строчная буква
        password.Append(chars[random.Next(52, 62)]); // Цифра
        password.Append(chars[random.Next(62, chars.Length)]); // Специальный символ

        for (int i = 0; i < 4; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }

        return password.ToString();
    }

    private async Task SendPasswordResetEmail(string email, string newPassword)
    {
        var emailBody = GeneratePasswordResetEmailBody(newPassword);
        await _emailService.SendPasswordResetAsync(email, emailBody);
    }

    private string GeneratePasswordResetEmailBody(string newPassword)
    {
        // Тот же код что и в ProfileController
        var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine("<html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
    sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
    
    sb.AppendLine("<div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; margin-bottom: 20px;'>");
    sb.AppendLine("<h1 style='color: #007bff; margin: 0;'>MoeShop</h1>");
    sb.AppendLine("<h2 style='color: #dc3545; margin: 10px 0 0 0;'>Восстановление пароля</h2>");
    sb.AppendLine("</div>");
    
    sb.AppendLine("<div style='background-color: #fff; border: 1px solid #dee2e6; border-radius: 8px; padding: 20px; margin-bottom: 20px;'>");
    sb.AppendLine("<p>Вы запросили восстановление пароля для вашего аккаунта в MoeShop.</p>");
    sb.AppendLine($"<p><strong>Ваш новый пароль:</strong> <span style='background-color: #f8f9fa; padding: 5px 10px; border-radius: 4px; font-family: monospace; font-size: 16px;'>{newPassword}</span></p>");
    sb.AppendLine("<p style='color: #dc3545;'><strong>Важно:</strong> Сохраните этот пароль в безопасном месте.</p>");
    sb.AppendLine("</div>");
    
    sb.AppendLine("<div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; color: #6c757d;'>");
    sb.AppendLine("<p style='margin: 5px 0;'>С уважением, команда MoeShop</p>");
    sb.AppendLine("<p style='margin: 5px 0; font-size: 12px;'>Это автоматическое сообщение, не отвечайте на него.</p>");
    sb.AppendLine("</div>");
    
    sb.AppendLine("</div>");
    sb.AppendLine("</body></html>");
    
    return sb.ToString();
}

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
