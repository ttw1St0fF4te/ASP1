namespace PR1.Models;

using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [StringLength(50)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}