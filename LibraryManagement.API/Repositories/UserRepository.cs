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

    public async Task<User?> GetByIdAsync(int userId) {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<User> CreateAsync(User user) {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateAsync(int userId, User user) {
        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (existingUser == null) return null;

        existingUser.Loans = user.Loans;
        existingUser.Email = user.Email;
        existingUser.FirstName = user.FirstName;
        existingUser.Name  = user.Name;
        existingUser.Fees = user.Fees;
        
        await _dbContext.SaveChangesAsync();
        return existingUser;
    }
    public async Task UpdateRangeAsync(List<User> users) {
        _dbContext.Users.UpdateRange(users);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> DeleteAsync(int userId) {
        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (existingUser == null) return null;

        _dbContext.Users.Remove(existingUser);
        await _dbContext.SaveChangesAsync();
        return existingUser;
    }
}