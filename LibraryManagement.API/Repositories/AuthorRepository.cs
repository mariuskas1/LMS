using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repositories;

public class AuthorRepository : IAuthorRepository {
    private readonly LmsDbContext _dbContext;

    public AuthorRepository(LmsDbContext dbContext) {
        _dbContext = dbContext;
    }
    
    public async Task<List<Author>> GetAllAsync() {
        return await _dbContext.Authors.ToListAsync();
    }

    public async Task<Author?> GetByIdAsync(int id) {
        return await _dbContext.Authors.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Author> CreateAsync(Author author) {
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();
        return author;
    }

    public async Task<Author?> UpdateAsync(int id, Author author) {
        Author? existingAuthor = await _dbContext.Authors.FirstOrDefaultAsync(x => x.Id == id);
        if (existingAuthor == null) return null;

        existingAuthor.Name = author.Name;
        await _dbContext.SaveChangesAsync();
        return existingAuthor;
    }

    public async Task<Author?> DeleteAsync(int id) {
        Author? existingAuthor = await _dbContext.Authors.FirstOrDefaultAsync(x => x.Id == id);
        if (existingAuthor == null) return null;

        _dbContext.Authors.Remove(existingAuthor);
        await _dbContext.SaveChangesAsync();
        return existingAuthor;
    }
}