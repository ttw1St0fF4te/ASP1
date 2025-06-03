using System.ComponentModel.DataAnnotations.Schema;

namespace PR1.Models;

public class Order
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime OrderDate { get; set; }
    
    // для контактных данных и адреса
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    
    // для работы с виртуальным кошельком
    public decimal TotalAmount { get; set; } // Общая сумма заказа
    public decimal? WalletUsed { get; set; } // Сумма списанная с кошелька
    public decimal? WalletEarned { get; set; } // Сумма начисленная на кошелек
    public decimal FinalAmount { get; set; } // Итоговая сумма к оплате

    public List<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }

    [ForeignKey("Order")]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; } // цена товара на момент заказа
}