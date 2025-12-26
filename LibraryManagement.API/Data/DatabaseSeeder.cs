using LibraryManagement.API.Clients;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Data;

public class DatabaseSeeder {
    private readonly LmsDbContext _dbContext;
    private readonly OpenLibraryClient _openLibraryClient;

    public DatabaseSeeder(LmsDbContext dbContext, OpenLibraryClient openLibraryClient) {
        _dbContext = dbContext;
        _openLibraryClient = openLibraryClient;
    }

    public async Task SeedDatabaseAsync() {
        if (await _dbContext.Books.CountAsync() > 100) return;
        
        List<Book> books = await _openLibraryClient.GetBooksBySubjectAsync("science fiction");

        _dbContext.Books.AddRange(books);
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAllBooksAsync() {
        DbSet<Book> books = _dbContext.Books;
        _dbContext.Books.RemoveRange(books);
        await _dbContext.SaveChangesAsync();
    }
}