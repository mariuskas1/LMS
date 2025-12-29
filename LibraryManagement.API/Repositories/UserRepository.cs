using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repositories;

public class UserRepository : IUserRepository {
    private readonly LmsDbContext _dbContext;

    public UserRepository(LmsDbContext dbContext) {
        _dbContext = dbContext;
    }
    
    public async Task<List<User>> GetAllAsync() {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id) {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User> CreateAsync(User user) {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateAsync(int id, User user) {
        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (existingUser == null) return null;

        existingUser.Loans = user.Loans;
        existingUser.Email = user.Email;
        existingUser.FirstName = user.FirstName;
        existingUser.Name  = user.Name;
        existingUser.OutstandingFees = user.OutstandingFees;
        
        await _dbContext.SaveChangesAsync();
        return existingUser;
    }

    public async Task<User?> DeleteAsync(int id) {
        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (existingUser == null) return null;

        _dbContext.Users.Remove(existingUser);
        await _dbContext.SaveChangesAsync();
        return existingUser;
    }
}