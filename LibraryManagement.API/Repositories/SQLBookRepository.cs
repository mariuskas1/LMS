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

    public async Task AddRangeAsync(IEnumerable<Book> books) {
        await _dbContext.Books.AddRangeAsync(books);
        await _dbContext.SaveChangesAsync();
    }

    public Task<int> CountAsync() 
        => _dbContext.Books.CountAsync();
    

    public async Task DeleteAllAsync() {
        DbSet<Book> books = _dbContext.Books;
        _dbContext.Books.RemoveRange(books);
        await _dbContext.SaveChangesAsync();
    }
}