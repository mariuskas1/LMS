using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Services;

public class FeeCalculationService : BackgroundService {
    private TimeSpan ExecutionInterval { get; } = TimeSpan.FromDays(1);

    private readonly IUserRepository _userRepository;
    private readonly FeeUpdateService _feeUpdateService;

    public FeeCalculationService(ILogger<FeeCalculationService> logger, IUserRepository userRepository, ILoanRepository loanRepository, FeeUpdateService feeUpdateService) {
        _userRepository = userRepository;
        _feeUpdateService = feeUpdateService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await RunFeeCalculationAsync();
            
            await Task.Delay(ExecutionInterval, stoppingToken);
        }
    }

    internal async Task RunFeeCalculationAsync() {
        List<User> allUsers = await _userRepository.GetAllAsync();
        
        CheckOverdueLoans(allUsers);

        await _userRepository.UpdateRangeAsync(allUsers);
    }

    /// <summary>
    /// Checks if a user has overdue loans and if so, adds a fee to the user's account according to the following model:
    /// Day 1-10: 0.50€ / day, from day 11: 1€ / day. Maximum fee per medium: 15€
    /// </summary>
    private void CheckOverdueLoans(List<User> users) {
        foreach (User user in users) {
            if (user.Loans.Count == 0) continue;

            List<Loan> overdueLoans = user.Loans.Where(loan => loan.IsOverdue(DateTime.Now)).ToList();
            if (overdueLoans.Count == 0) continue;

            foreach (Loan loan in overdueLoans) {
                int daysOverdue = (DateTime.Now - loan.DueAt).Days;
                decimal amount = daysOverdue <= 10 ? 0.5m : 1.0m;
                Fee overdueFee = Fee.CreateOverdueFee(amount, loan.Id);
                _feeUpdateService.TryAddFee(overdueFee, user, loan);
            }
        }
    }
}