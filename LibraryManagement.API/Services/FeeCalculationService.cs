using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Services;

public class FeeCalculationService : BackgroundService {
    private TimeSpan ExecutionInterval { get; } = TimeSpan.FromDays(7);
    
    private readonly ILogger<FeeCalculationService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ILoanRepository _loanRepository;

    public FeeCalculationService(ILogger<FeeCalculationService> logger, IUserRepository userRepository, ILoanRepository loanRepository) {
        _logger = logger;
        _userRepository = userRepository;
        _loanRepository = loanRepository;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        
        while (!stoppingToken.IsCancellationRequested) {
            await RunFeeCalculationAsync();
            
            await Task.Delay(ExecutionInterval, stoppingToken);
        }
    }

    private async Task RunFeeCalculationAsync() {
        List<User> allUsers = await _userRepository.GetAllAsync();
       
        CheckAnnualFees(allUsers);
        CheckRemindedOutstandingFees(allUsers);
        CheckOverdueLoans(allUsers);
        
    }

    /// <summary> Checks if a user's annual fee is overdue and if so, adds a new annual fee to the user's account. </summary>
    private void CheckAnnualFees(List<User> users) {
        foreach (User user in users) {
            if (!(user.AnnualFeeDue > DateTime.Now)) continue;

            Fee newAnnualFee = new() { Amount = 30, Reason = "Annual fee" };
            user.OutstandingFees.Add(newAnnualFee);
            _logger.LogInformation("Fees have been updated with annual fee for the user {user.Id}.", user.Id);
        }
    }

    /// <summary> Checks if a user has outstanding fees that already have been reminded once or multiple times and if so, adds new additional fees to the user's account.
    /// according to the following model: 1€ for the first reminder, 3€ for the second reminder.</summary>
    private static void CheckRemindedOutstandingFees(List<User> users) {
        // TODO: Iterate over overdue loans for each user
        // Or maybe do this in the iteration below...
    }

    /// <summary> Checks if a user has overdue loans and if so, adds a fee to the user's account. </summary>
    /// <param name="users"></param>
    private void CheckOverdueLoans(List<User> users) {
        foreach (User user in users) {
            if (user.Loans.Count == 0) continue;

            List<Loan> overdueLoans = user.Loans.Where(loan => loan.IsOverdue(DateTime.Now)).ToList();
            if (overdueLoans.Count == 0) continue;

            foreach (Loan loan in overdueLoans) {
                // Calculate general overdue fine:
                int daysOverdue = (DateTime.Now.Date - loan.DueAt.Date).Days;
                if (daysOverdue <= 0) continue;
                
                decimal feeAmount = daysOverdue * 0.4m;
                Fee newFee = new() { Amount = feeAmount, Reason = $"Fee for the overdue loan {loan.Id}. Days overdue: {daysOverdue}" };
                user.OutstandingFees.Add(newFee);
                _logger.LogInformation("Fees have been updated with overdue fee for the user {user.Id} with an amount of {feeAmount}€.", user.Id, feeAmount);
                
                // Calculate reminder fee:
                if (loan.TimesReminded == 0) continue;

                if (loan.TimesReminded == 1) {
                    Fee newReminderFee = new() { Amount = 1.0m, Reason = $"First reminder fee for the overdue loan {loan.Id}." };
                    user.OutstandingFees.Add(newReminderFee);
                    _logger.LogInformation("Fees have been updated with a first reminder fee for the user {user.Id}.", user.Id);
                }
                
                if (loan.TimesReminded == 2) {
                    Fee newReminderFee = new() { Amount = 3.0m, Reason = $"Second reminder fee for the overdue loan {loan.Id}." };
                    user.OutstandingFees.Add(newReminderFee);
                    _logger.LogInformation("Fees have been updated with a second reminder fee for the user {user.Id}.", user.Id);
                }
                
            }
        }
    }
}