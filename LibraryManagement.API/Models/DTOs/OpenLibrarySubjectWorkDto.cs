using System.Text.Json.Serialization;

namespace LibraryManagement.API.Models.DTOs;

public class OpenLibrarySubjectWorkDto {
    [JsonPropertyName("key")]
    public string? Key { get; set; }            

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }

    [JsonPropertyName("edition_count")]
    public int? EditionCount { get; set; }

    [JsonPropertyName("authors")]
    public List<OpenLibraryWorkAuthorDto>? Authors { get; set; }

    [JsonPropertyName("cover_id")]
    public long? CoverId { get; set; }

    [JsonPropertyName("subject")]
    public List<string>? Subjects { get; set; }
}