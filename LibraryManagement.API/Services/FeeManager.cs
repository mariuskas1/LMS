using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Services;

/// <summary> Handles adding fees to users and loans. </summary>
public class FeeManager {
    private readonly ILogger<FeeManager> _logger;

    public FeeManager(ILogger<FeeManager> logger) {
        _logger = logger;
    }

    /// <summary> Adds an annual fee to the given user. </summary>
    public void AddAnnualFee(User user) {
        Fee newAnnualFee = Fee.CreateAnnualFee(DateTime.Now.Year);
        user.Fees.Add(newAnnualFee);
        _logger.LogInformation("Annual fee added for user {UserId}.", user.Id);
    }
    
    /// <summary> Adds a first reminder fee (1€) subject to the loan. </summary>
    public void AddFirstReminderFee(User user, Loan loan) {
        Fee reminderFee = Fee.CreateFirstReminderFee(loan.Id);
        TryAddFee(reminderFee, user, loan);
        _logger.LogInformation("First reminder fee added for user {UserId}.", user.Id);
    }

    /// <summary> Adds a second reminder fee (3€) to the loan. </summary>
    public void AddSecondReminderFee(User user, Loan loan) {
        Fee reminderFee = Fee.CreateSecondReminderFee(loan.Id);
        TryAddFee(reminderFee, user, loan);
        _logger.LogInformation("Second reminder fee added for user {UserId}.", user.Id);
    }

    /// <summary>
    /// Adds a fee to the given user and loan object and updates the loan repository.
    /// Pays attention that the maximum amount per loan of 15€ is kept so the fee amount might be reduced.
    /// If the fee amount of the given loan is already at 15€ or above, no fee is added to the user or loan.
    /// </summary>
    public void TryAddFee(Fee fee, User user, Loan loan) {
        if (loan.FeeAmount + fee.Amount >= 15) {
            decimal remaining = 15 - loan.FeeAmount;

            if (remaining <= 0) {
                _logger.LogInformation("Fee cap already reached for loan {LoanId}. No fee added.", loan.Id);
                return;
            }

            fee.Amount = remaining;
            fee.Description += $" Fee reduced to {remaining}€ to stay within the 15€ cap.";
            _logger.LogInformation("Fee reduced to cap for loan {LoanId}.", loan.Id);
        }

        user.Fees.Add(fee);
        loan.Fees.Add(fee);
    }
}