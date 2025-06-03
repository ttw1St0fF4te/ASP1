using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using PR1.Models;

namespace PR1.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(string toEmail, Order order)
    {
        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.Port,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = $"Подтверждение заказа #{order.Id} - MoeShop",
                Body = GenerateEmailBody(order),
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            
            _logger.LogInformation($"Отправка email на {toEmail} для заказа #{order.Id}");
            
            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation($"Email успешно отправлен на {toEmail} для заказа #{order.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка отправки email на {toEmail} для заказа #{order.Id}: {ex.Message}");
            // В зависимости от требований, можно выбросить исключение или обработать тихо
            // throw; // Раскомментируйте, если хотите прокинуть исключение выше
        }
    }

    private string GenerateEmailBody(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        
        // Заголовок
        sb.AppendLine("<div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; margin-bottom: 20px;'>");
        sb.AppendLine("<h1 style='color: #007bff; margin: 0;'>MoeShop</h1>");
        sb.AppendLine("<h2 style='color: #28a745; margin: 10px 0 0 0;'>Спасибо за покупку!</h2>");
        sb.AppendLine("</div>");
        
        // Информация о заказе
        sb.AppendLine("<div style='background-color: #fff; border: 1px solid #dee2e6; border-radius: 8px; padding: 20px; margin-bottom: 20px;'>");
        sb.AppendLine($"<p><strong>Заказ:</strong> #{order.Id}</p>");
        sb.AppendLine($"<p><strong>Дата заказа:</strong> {order.OrderDate:dd.MM.yyyy HH:mm}</p>");
        sb.AppendLine("<p style='color: #28a745;'><strong>Ваш заказ успешно оформлен и принят в обработку!</strong></p>");
        sb.AppendLine("</div>");
        
        // Товары в заказе
        sb.AppendLine("<div style='background-color: #fff; border: 1px solid #dee2e6; border-radius: 8px; padding: 20px; margin-bottom: 20px;'>");
        sb.AppendLine("<h3 style='color: #007bff; margin-top: 0;'>Товары в заказе:</h3>");
        sb.AppendLine("<table style='width: 100%; border-collapse: collapse;'>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr style='background-color: #f8f9fa;'>");
        sb.AppendLine("<th style='text-align: left; padding: 10px; border-bottom: 1px solid #dee2e6;'>Товар</th>");
        sb.AppendLine("<th style='text-align: center; padding: 10px; border-bottom: 1px solid #dee2e6;'>Кол-во</th>");
        sb.AppendLine("<th style='text-align: right; padding: 10px; border-bottom: 1px solid #dee2e6;'>Цена</th>");
        sb.AppendLine("<th style='text-align: right; padding: 10px; border-bottom: 1px solid #dee2e6;'>Сумма</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");
        
        foreach (var item in order.OrderItems)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style='padding: 10px; border-bottom: 1px solid #f1f3f4;'>{item.Product.Name}</td>");
            sb.AppendLine($"<td style='text-align: center; padding: 10px; border-bottom: 1px solid #f1f3f4;'>{item.Quantity} шт.</td>");
            sb.AppendLine($"<td style='text-align: right; padding: 10px; border-bottom: 1px solid #f1f3f4;'>{item.PriceAtOrder:F2} ₽</td>");
            sb.AppendLine($"<td style='text-align: right; padding: 10px; border-bottom: 1px solid #f1f3f4;'><strong>{(item.Quantity * item.PriceAtOrder):F2} ₽</strong></td>");
            sb.AppendLine("</tr>");
        }
        
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");
        
        // Итоговая информация
        sb.AppendLine("<div style='background-color: #f8f9fa; border-radius: 8px; padding: 20px;'>");
        sb.AppendLine($"<p style='font-size: 18px; margin: 5px 0;'><strong>Общая сумма: {order.TotalAmount:F2} ₽</strong></p>");
        
        if (order.WalletUsed.HasValue && order.WalletUsed > 0)
        {
            sb.AppendLine($"<p style='color: #dc3545; margin: 5px 0;'>Списано с кошелька: {order.WalletUsed:F2} ₽</p>");
        }
        
        if (order.WalletEarned.HasValue && order.WalletEarned > 0)
        {
            sb.AppendLine($"<p style='color: #28a745; margin: 5px 0;'>Начислено на кошелек: +{order.WalletEarned:F2} ₽</p>");
        }
        
        sb.AppendLine($"<p style='font-size: 20px; color: #007bff; margin: 10px 0 0 0;'><strong>К оплате: {order.FinalAmount:F2} ₽</strong></p>");
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