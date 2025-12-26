using System.Text.Json;
using AutoMapper;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Models.DTOs;

namespace LibraryManagement.API.Clients;

public class OpenLibraryClient {
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
    };

    public OpenLibraryClient(IMapper mapper) {
        _mapper = mapper;
    }

    public async Task<List<Book>> GetBooksBySubjectAsync(string subject) {
        string requestUrl =  $"https://openlibrary.org/subjects/{subject}.json?limit=100&offset=40";
        
        string json = await _httpClient.GetStringAsync(requestUrl);

        OpenLibrarySubjectResponseDto? response = JsonSerializer.Deserialize<OpenLibrarySubjectResponseDto>(json);

        if (response?.Works == null) {
            return [];
        }
        
        return _mapper.Map<List<Book>>(response.Works);
    }
}