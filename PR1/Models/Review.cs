using System.ComponentModel.DataAnnotations.Schema;

namespace PR1.Models;

public class Review
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int Rating { get; set; } // от 1 до 5
    public DateTime Date { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }
    public Product Product { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }
}