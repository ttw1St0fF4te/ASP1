using System.ComponentModel.DataAnnotations.Schema;

namespace PR1.Models;

public class User 
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    
    // для оформления заказа и системы лояльности
    public string? Email { get; set; } 
    public decimal? TotalSpent { get; set; }
    public decimal? WalletBalance { get; set; }
    public string? LoyaltyLevel { get; set; } 

    [ForeignKey("UserRole")]
    public int UserRoleId { get; set; }
    public UserRole UserRole { get; set; }
}