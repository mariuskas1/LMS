using LibraryManagement.API.Clients;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Data;

public class DatabaseSeeder {
    private readonly IBookRepository _bookRepository;
    private readonly OpenLibraryClient _openLibraryClient;

    public DatabaseSeeder(IBookRepository bookRepository, OpenLibraryClient openLibraryClient) {
        _bookRepository = bookRepository;
        _openLibraryClient = openLibraryClient;
    }
    
    public async Task SeedDatabaseAsync() {
        if (await _bookRepository.CountAsync() > 100) return;
        
        List<Book> books = await _openLibraryClient.GetBooksBySubjectAsync("science fiction", 100);

        await _bookRepository.AddRangeAsync(books);
    }

    public async Task DeleteAllBooksAsync() {
        await _bookRepository.DeleteAllAsync();
    }
}