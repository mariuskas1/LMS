using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Repositories;

public interface IBookRepository {
    
    Task<List<Book>> GetAllAsync();

    Task<Book?> GetByIdAsync(int bookId);

    Task<Book> CreateAsync(Book book);

    Task<Book?> UpdateAsync(int bookId, Book book);

    Task<Book?> DeleteAsync(int bookId);
    
    Task AddRangeAsync(IEnumerable<Book> books);
    
    Task<int> CountAsync();

    Task DeleteAllAsync();
}