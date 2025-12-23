using System.Text.Json;

namespace LibraryManagement.API.Clients;

public class OpenLibraryClient {
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
    };

    public async Task CallSubjectApi(string subject) {
        string requestUrl =  $"https://openlibrary.org/subjects/{subject}.json?limit=20&offset=40";
        
        var response = await _httpClient.GetAsync(requestUrl);          
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        
        
        
    }
}