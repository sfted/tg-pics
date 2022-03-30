namespace TgPics.Core.Models.Requests;

using System.ComponentModel.DataAnnotations;

public class UsersAuthRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}