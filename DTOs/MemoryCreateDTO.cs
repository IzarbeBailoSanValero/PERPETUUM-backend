using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PERPETUUM.DTOs;

public class MemoryCreateDTO
{
    [Required]
    [JsonPropertyName("deceasedId")]
    public int DeceasedId { get; set; }

    [Required]
    [JsonPropertyName("type")]
    public int Type { get; set; }  // 1: Condolence, 2: Anecdote, 3: Photo

    [JsonPropertyName("textContent")]
    public string? TextContent { get; set; }

    [JsonPropertyName("mediaURL")]
    public string? MediaURL { get; set; }

    [JsonPropertyName("authorRelation")]
    public string? AuthorRelation { get; set; }
}