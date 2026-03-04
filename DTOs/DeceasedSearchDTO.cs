namespace PERPETUUM.DTOs;

public class DeceasedSearchDTO
{
    public string? Name { get; set; }   
    public int? DeathYear { get; set; }  
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}