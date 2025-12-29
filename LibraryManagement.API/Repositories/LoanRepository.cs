using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repositories;

public class LoanRepository : ILoanRepository {
    private readonly LmsDbContext _dbContext;

    public LoanRepository(LmsDbContext dbContext) {
        _dbContext = dbContext;
    }
    
    public async Task<List<Loan>> GetAllAsync() {
        return await _dbContext.Loans.ToListAsync();
    }

    public async Task<Loan?> GetByIdAsync(int id) {
        return await _dbContext.Loans.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Loan> CreateAsync(Loan loan) {
        await _dbContext.Loans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();
        
        return loan;
    }

    public async Task<Loan?> UpdateAsync(int id, Loan loan) {
        Loan? existingLoan = await _dbContext.Loans.FirstOrDefaultAsync(x => x.Id == id);
        if (existingLoan == null) return null;

        existingLoan.DueAt = loan.DueAt;
        existingLoan.ReturnedAt = loan.ReturnedAt;
        await _dbContext.SaveChangesAsync();
        return existingLoan;
    }

    public async Task<Loan?> DeleteAsync(int id) {
        Loan? existingLoan = await _dbContext.Loans.FirstOrDefaultAsync(x => x.Id == id);
        if (existingLoan == null) return null;

        _dbContext.Loans.Remove(existingLoan);
        await _dbContext.SaveChangesAsync();
        return existingLoan;
    }
}