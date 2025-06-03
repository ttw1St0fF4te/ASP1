using PR1.Models;

namespace PR1.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string toEmail, Order order);
}