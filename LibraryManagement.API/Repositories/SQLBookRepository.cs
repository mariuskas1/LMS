using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repositories;

public class SqlBookRepository : IBookRepository {
    private readonly LmsDbContext _dbContext;

    public SqlBookRepository(LmsDbContext dbContext) {
        _dbContext = dbContext;
    }
    
    public async Task<List<Book>> GetAllAsync() {
        return await _dbContext.Books.ToListAsync();
    }

    public async Task<Book> CreateAsync(Book book) {
       await _dbContext.Books.AddAsync(book);
       await _dbContext.SaveChangesAsync();
       return book;
    }

    public async Task DeleteAllBooksAsync() {
        DbSet<Book> books = _dbContext.Books;
        _dbContext.Books.RemoveRange(books);
        await _dbContext.SaveChangesAsync();
    }
}