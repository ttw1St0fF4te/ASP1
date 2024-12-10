using System.ComponentModel.DataAnnotations.Schema;

namespace PR1.Models;

public class User 
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    [ForeignKey("UserRole")]
    public int UserRoleId { get; set; }
    public UserRole UserRole { get; set; }
}