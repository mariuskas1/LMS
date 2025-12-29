using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Repositories;

public interface ILoanRepository {
    Task<List<Loan>> GetAllAsync();

    Task<Loan?> GetByIdAsync(int id);

    Task<Loan> CreateAsync(Loan loan);

    Task<Loan?> UpdateAsync(int id, Loan loan);

    Task<Loan?> DeleteAsync(int id);
}