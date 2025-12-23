using System.Text.Json.Serialization;

namespace LibraryManagement.API.Models.DTOs;

public class OpenLibraryWorkAuthorDto {
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }     
}