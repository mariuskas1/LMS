using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repositories;

public class SQLBookRepository : IBookRepository {
    private readonly LmsDbContext _dbContext;

    public SQLBookRepository(LmsDbContext dbContext) {
        _dbContext = dbContext;
    }
    
    public async Task<List<Book>> GetAllAsync() {
        return await _dbContext.Books.ToListAsync();
    }
}