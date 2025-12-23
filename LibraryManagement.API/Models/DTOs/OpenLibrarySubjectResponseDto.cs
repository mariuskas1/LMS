using System.Text.Json.Serialization;

namespace LibraryManagement.API.Models.DTOs;

public class OpenLibrarySubjectResponseDto {
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("work_count")]
    public int WorkCount { get; set; }

    [JsonPropertyName("works")]
    public List<OpenLibrarySubjectWorkDto>? Works { get; set; }
}