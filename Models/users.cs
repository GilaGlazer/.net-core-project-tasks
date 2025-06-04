using webApiProject.Interfaces;

namespace webApiProject.Models;

public class Users
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Type { get; set; } = "user";
}
