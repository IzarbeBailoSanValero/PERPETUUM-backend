namespace PERPETUUM.Models;

/*
Status (INT):
0 - Pending, 
1 - Approved, 
2 - Rejected

Type (INT):
1 - Condolence (Text only), 
2 - Anecdote (Text), 
3 - Photo (MediaURL)
*/
public class Memory
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int Type { get; set; }
    public int Status { get; set; } = 0; // default Pending (entra por defecot como pendiente desde el front sin que lo mande)
    public string? TextContent { get; set; } //can be null
    public string? MediaURL { get; set; }//can be null
    public string? AuthorRelation { get; set; }//can be null
    public int DeceasedId { get; set; }
    public int UserId { get; set; }
}