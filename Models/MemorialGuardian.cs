namespace PERPETUUM.Models;

public class MemorialGuardian
{
    public int Id { get; set; }
    public int FuneralHomeId { get; set; }
    public int StaffId { get; set; } 

    public string Name { get; set; }
    public string DNI { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public List<Deceased> ManagedMemorials { get; set; }
}