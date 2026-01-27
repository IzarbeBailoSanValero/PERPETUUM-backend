namespace PERPETUUM.Models;

public class Staff
{
    public int Id { get; set; }
    public int FuneralHomeId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string DNI { get; set; }

    public string PasswordHash { get; set; }
    public bool IsAdmin { get; set; }
    public List<Deceased> DeceasedInCharge { get; set; }
}