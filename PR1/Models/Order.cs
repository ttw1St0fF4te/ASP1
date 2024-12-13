using System.ComponentModel.DataAnnotations.Schema;

namespace PR1.Models;

public class Order
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime OrderDate { get; set; }

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
}