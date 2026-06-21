using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Services;

public class NotificationService : BackgroundService{
    private TimeSpan ExecutionInterval { get; } = TimeSpan.FromDays(1);

    private readonly IUserRepository _userRepository;
    private readonly FeeManager _feeManager;

    public NotificationService(IUserRepository userRepository, FeeManager feeManager) {
        _userRepository = userRepository;
        _feeManager = feeManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await RunNotificationCheckAsync();
            await Task.Delay(ExecutionInterval, stoppingToken);
        }
    }
    
    internal async Task RunNotificationCheckAsync() {
        List<User> allUsers = await _userRepository.GetAllAsync();
        
        CheckAnnualFees(allUsers);
        CheckOverdueLoans(allUsers);
        
        await _userRepository.UpdateRangeAsync(allUsers);
    }
    
    /// <summary> Checks if a user's annual fee is overdue and if so, adds a new annual fee to the user's account. </summary>
    private void CheckAnnualFees(List<User> users) {
        foreach (User user in users) {
            if (!AnnualFeeDue(user)) continue;

            SendAnnualFeeNotification(user);
            _feeManager.AddAnnualFee(user);
        }
    }

    private void CheckOverdueLoans(List<User> users) {
        foreach (User user in users) {
            if (user.Loans.Count == 0) continue;

            List<Loan> overdueLoans = user.Loans.Where(loan => loan.IsOverdue(DateTime.Now)).ToList();
            if (overdueLoans.Count == 0) continue;

            foreach (Loan loan in overdueLoans) {
                bool isLoanOverdueFiveDays = (DateTime.Now - loan.DueAt)>= TimeSpan.FromDays(5);
                bool isLoandOverdueTenDays = (DateTime.Now - loan.DueAt) >= TimeSpan.FromDays(10);

                if (isLoandOverdueTenDays && !HasSecondReminderFee(loan)) {
                    SendSecondReminderNotification(user);
                    loan.TimesReminded++;
                    _feeManager.AddSecondReminderFee(user, loan);
                    continue;
                }
                
                if (isLoanOverdueFiveDays && !HasFirstReminderFee(loan)) {
                    SendFirstReminderNotification(user);
                    loan.TimesReminded++;
                    _feeManager.AddFirstReminderFee(user, loan);
                } 
            }
        }
    }

    private void SendFirstReminderNotification(User user) {
        
    }
    
    void SendSecondReminderNotification(User user) {
        
    }



    private void SendAnnualFeeNotification(User user) {
        
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
    private static bool AnnualFeeAlreadyAdded(User user) 
        => user.Fees.Any(fee => fee.Type == FeeType.Annual && fee.CreatedAt.Year == DateTime.Now.Year);
    
    /// <summary> Checks if a first reminder fee has already been added to a loan. </summary>
    private static bool HasFirstReminderFee(Loan loan) {
        return loan.Fees.Any(fee => fee.Type == FeeType.FirstReminder);
    }

    /// <summary> Checks if a second reminder fee has already been added to a loan. </summary>
    private static bool HasSecondReminderFee(Loan loan) {
        return loan.Fees.Any(fee => fee.Type == FeeType.SecondReminder);
    }
}