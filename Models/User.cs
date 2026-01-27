namespace PERPETUUM.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; }

    public List<Memory> MyMemoriesSent { get; set; }
}