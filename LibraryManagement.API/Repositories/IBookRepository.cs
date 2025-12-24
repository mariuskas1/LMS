using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Repositories;

public interface IBookRepository {
    
    Task<List<Book>> GetAllAsync();
    
    Task<Book> CreateAsync(Book book);
}