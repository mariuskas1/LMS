using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Repositories;

public interface IUserRepository {
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int userId);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(int userId, User user);
    Task UpdateRangeAsync(List<User> users);
    Task<User?> DeleteAsync(int userId);
}