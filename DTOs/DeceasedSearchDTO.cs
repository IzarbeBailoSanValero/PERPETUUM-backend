namespace PERPETUUM.DTOs;

public class DeceasedSearchDTO
{
    public string? Name { get; set; }   
    public int? DeathYear { get; set; }  
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "ASC";
    public int Page { get; set; } = 1;
}