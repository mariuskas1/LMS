using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repositories;

public class BookRepository : IBookRepository {
    private readonly LmsDbContext _dbContext;

    public BookRepository(LmsDbContext dbContext) {
        _dbContext = dbContext;
    }
    
    public async Task<List<Book>> GetAllAsync() {
        return await _dbContext.Books.ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id) {
        return await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Book> CreateAsync(Book book) {
       await _dbContext.Books.AddAsync(book);
       await _dbContext.SaveChangesAsync();
       return book;
    }

    public async Task<Book?> UpdateAsync(int id, Book book) {
        Book? existingBook = await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id);
        if (existingBook == null) return null;

        existingBook.Authors = book.Authors;
        existingBook.Title = book.Title;
        existingBook.Subtitle = book.Subtitle;
        existingBook.PublishYear = book.PublishYear;
        existingBook.IsBorrowed = book.IsBorrowed;

        await _dbContext.SaveChangesAsync();
        
        return existingBook;
    }

    public async Task<Book?> DeleteAsync(int id) {
        Book? existingBook = await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id);
        if (existingBook == null) return null;
        
        _dbContext.Books.Remove(existingBook);
        await _dbContext.SaveChangesAsync();
        return existingBook;        
    }

    public async Task AddRangeAsync(IEnumerable<Book> books) {
        await _dbContext.Books.AddRangeAsync(books);
        await _dbContext.SaveChangesAsync();
    }

    public Task<int> CountAsync() 
        => _dbContext.Books.CountAsync();
    

    public async Task DeleteAllAsync() {
        DbSet<Author> authors = _dbContext.Authors;
        _dbContext.Authors.RemoveRange(authors);
        await _dbContext.SaveChangesAsync();
        
        DbSet<Book> books = _dbContext.Books;
        _dbContext.Books.RemoveRange(books);
        await _dbContext.SaveChangesAsync();
    }
}