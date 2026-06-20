using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Repositories;

public interface ILoanRepository {
    Task<List<Loan>> GetAllAsync();
    
    Task<List<Loan>> GetAllActiveLoansAsync();

    Task<Loan?> GetByIdAsync(int loanId);

    Task<Loan> CreateAsync(Loan loan);

    Task<Loan?> UpdateAsync(int loanId, Loan loan);

    Task<Loan?> DeleteAsync(int loanId);
}