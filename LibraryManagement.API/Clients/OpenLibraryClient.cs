using System.Text.Json;
using AutoMapper;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Models.DTOs;

namespace LibraryManagement.API.Clients;

/// <summary> Holds the logic to get book data from the Open Library API. </summary>
public class OpenLibraryClient {
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
    };
    
    public OpenLibraryClient(IMapper mapper) {
        _mapper = mapper;
    }

    /// <summary> Fetches book data for the given subject and maps the data from the DTO to the book domain model. </summary>
    /// <param name="subject"> The subject that detetmines what kind of books will be fetched. </param>
    /// <param name="limit"> Determines how many books will be fetched. </param>
    /// <returns></returns>
    public async Task<List<Book>> GetBooksBySubjectAsync(string subject, int limit) {
        string requestUrl =  $"https://openlibrary.org/subjects/{subject}.json?limit={limit}&offset=40";
        
        string json = await _httpClient.GetStringAsync(requestUrl);

        OpenLibrarySubjectResponseDto? response = JsonSerializer.Deserialize<OpenLibrarySubjectResponseDto>(json);

        if (response?.Works == null) {
            return [];
        }
        
        return _mapper.Map<List<Book>>(response.Works);
    }
}