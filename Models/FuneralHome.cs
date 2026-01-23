namespace PERPETUUM.Models;

public class FuneralHome
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CIF { get; set; }
    public string ContactEmail { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    
    public List<Staff> ListStaff { get; set; }
    public List<Deceased> ListDeceased { get; set; }

   
}