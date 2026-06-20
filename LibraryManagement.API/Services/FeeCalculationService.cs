using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Services;

public class FeeCalculationService : BackgroundService {
    private TimeSpan ExecutionInterval { get; } = TimeSpan.FromDays(1);
    private readonly int _currentYear = DateTime.Now.Year;
    
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
        await CheckOverdueLoans(allUsers);

        await _userRepository.UpdateRangeAsync(allUsers);
    }

    /// <summary> Checks if a user's annual fee is overdue and if so, adds a new annual fee to the user's account. </summary>
    private void CheckAnnualFees(List<User> users) {
        foreach (User user in users) {
            if (!AnnualFeeDue(user)) continue;

            Fee newAnnualFee = new() { Amount = 30, Reason = $"Annual fee for year {_currentYear}", Type = FeeType.Annual };
            user.Fees.Add(newAnnualFee);
            _logger.LogInformation("Fees have been updated with annual fee for the user {user.Id}.", user.Id);
            // TODO: Call FeeNotificationService
        }
    }

    /// <summary> Checks if a user has overdue loans and if so, adds a fee to the user's account according to the following model:
    /// General overdue fee:
    ///     Day 1-10: 0.50€ / day
    ///     From day 11: 1€ / day
    ///
    /// Reminder fee:
    ///     First reminder: 1€
    ///     Second reminder: 3€
    ///
    /// Maximum fee per medium: 15€
    /// </summary>
    private async Task CheckOverdueLoans(List<User> users) {
        foreach (User user in users) {
            if (user.Loans.Count == 0) continue;

            List<Loan> overdueLoans = user.Loans.Where(loan => loan.IsOverdue(DateTime.Now)).ToList();
            if (overdueLoans.Count == 0) continue;

            foreach (Loan loan in overdueLoans) {
                // Calculate general overdue fee:
                int daysOverdue = (DateTime.Now - loan.DueAt).Days;
                if (daysOverdue <= 10) {
                    Fee overdueFee = Fee.CreateOverdueFee(0.5m, loan.Id);
                    await AddFee(overdueFee, user, loan);
                } else {
                    Fee overdueFee = Fee.CreateOverdueFee(1m, loan.Id);
                    await AddFee(overdueFee, user, loan);
                }
                
                // Calculate reminder fee:
                if (loan.TimesReminded == 0) continue;

                if (loan.TimesReminded == 1 && !HasFirstReminderFee(loan)) {
                    Fee newReminderFee = Fee.CreateFirstReminderFee(loan.Id);
                    await AddFee(newReminderFee, user, loan);
                    _logger.LogInformation("Fees have been updated with a first reminder fee for the user {user.Id}.", user.Id);
                } else if (loan.TimesReminded == 2 && !HasSecondReminderFee(loan)) {
                    Fee newReminderFee = Fee.CreateSecondReminderFee(loan.Id);
                    await AddFee(newReminderFee, user, loan);
                    _logger.LogInformation("Fees have been updated with a second reminder fee for the user {user.Id}.", user.Id);
                }
            }
        }
    }

    /// <summary>
    /// Checks if an annual fee for the user is due.
    /// This is true if one year has elapsed since the last annual fee or since the accession date of the user.
    /// </summary>
    private bool AnnualFeeDue(User user) {
        Fee? lastAnnualFee = user.Fees
                                    .Where(fee => fee.Type == FeeType.Annual)
                                    .MaxBy(fee => fee.CreatedAt);
        
        DateTime referenceDate = lastAnnualFee?.CreatedAt ?? user.AccessionDate;

        bool hasOneYearElapsedSinceReferenceDate = DateTime.Now >= referenceDate.AddYears(1);

        return hasOneYearElapsedSinceReferenceDate && !AnnualFeeAlreadyAdded(user);
    }

    /// <summary> Checks if an annual fee for the user has already been added for the current year. </summary>
    private bool AnnualFeeAlreadyAdded(User user) 
        => user.Fees.Any(fee => fee.Type == FeeType.Annual && fee.CreatedAt.Year == _currentYear);

    /// <summary> Checks if a first reminder fee has already been added to a loan. </summary>
    private static bool HasFirstReminderFee(Loan loan) {
        return loan.Fees.Any(fee => fee.Type == FeeType.FirstReminder);
    }

    /// <summary> Checks if a second reminder fee has already been added to a loan. </summary>
    private static bool HasSecondReminderFee(Loan loan) {
        return loan.Fees.Any(fee => fee.Type == FeeType.SecondReminder);
    }


    /// <summary>
    /// Adds a fee to the given user and loan object and updates the loan repository.
    /// Pays attention that the maximum amount per loan of 15€ is kept so the fee amount might be reduced.
    /// If the fee amount of the given loan is already at 15€, no fee is added to the user or loan.
    /// </summary>
    private async Task AddFee(Fee fee, User user, Loan loan) {
        if (loan.FeeAmount + fee.Amount >= 15) {
            decimal difference = 15 - (loan.FeeAmount + fee.Amount);

            if (difference <= 0) {
                _logger.LogInformation("The maximum fee amount has already been reached. No fee was added.");
                return;
            }
            
            fee.Amount = difference;
            fee.Reason += $" The fee amount was cut to {difference}€ so the fee maximum of 15€ per medium is kept.";
            _logger.LogInformation("The maximum fee amount has been reached.");
        }
        
        user.Fees.Add(fee);
        loan.Fees.Add(fee);
        
        await _loanRepository.UpdateAsync(loan.Id, loan);
        
        //TODO: Call FeeNotificationService
    }
}