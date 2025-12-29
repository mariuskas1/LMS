using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Repositories;

public interface IBookRepository {
    
    Task<List<Book>> GetAllAsync();

    Task<Book?> GetByIdAsync(int id);

    Task<Book> CreateAsync(Book book);

    Task<Book?> UpdateAsync(int id, Book book);

    Task<Book?> DeleteAsync(int id);
    
    Task AddRangeAsync(IEnumerable<Book> books);
    
    Task<int> CountAsync();

    Task DeleteAllAsync();
}