using LibraryManagement.API.Mail;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LibraryManagement.API.Services;

public class NotificationService : BackgroundService{
    private TimeSpan ExecutionInterval { get; } = TimeSpan.FromDays(1);

    private readonly IUserRepository _userRepository;
    private readonly FeeUpdateService _feeUpdateService;
    private readonly ILogger<NotificationService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public NotificationService(IUserRepository userRepository, FeeUpdateService feeUpdateService, ILogger<NotificationService> logger, SmtpSettings smtpSettings) {
        _userRepository = userRepository;
        _feeUpdateService = feeUpdateService;
        _logger = logger;
        _smtpSettings = smtpSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await RunNotificationCheckAsync();
            await Task.Delay(ExecutionInterval, stoppingToken);
        }
    }
    
    internal async Task RunNotificationCheckAsync() {
        List<User> allUsers = await _userRepository.GetAllAsync();
        await CheckAnnualFees(allUsers);
        await CheckOverdueLoans(allUsers);
        await _userRepository.UpdateRangeAsync(allUsers);
    }
    
    /// <summary> Checks if a user's annual fee is overdue and if so, adds a new annual fee to the user's account. </summary>
    private async Task CheckAnnualFees(List<User> users) {
        foreach (User user in users) {
            if (!AnnualFeeDue(user)) continue;
            
            await SendAnnualFeeNotification(user);
            _feeUpdateService.AddAnnualFee(user);
        }
    }

    private async Task CheckOverdueLoans(List<User> users) {
        foreach (User user in users) {
            if (user.Loans.Count == 0) continue;

            List<Loan> overdueLoans = user.Loans.Where(loan => loan.IsOverdue(DateTime.Now)).ToList();
            if (overdueLoans.Count == 0) continue;

            foreach (Loan loan in overdueLoans) {
                bool isLoanOverdueFiveDays = (DateTime.Now - loan.DueAt)>= TimeSpan.FromDays(5);
                bool isLoanOverDueTenDays = (DateTime.Now - loan.DueAt) >= TimeSpan.FromDays(10);

                if (isLoanOverDueTenDays && !HasReminderFee(loan, FeeType.SecondReminder)) {
                    await HandleSecondReminderDue(user, loan);
                } else if (isLoanOverdueFiveDays && !HasReminderFee(loan, FeeType.FirstReminder)) {
                    await HandleFirstReminderDue(user, loan);
                }
            }
        }
    }

    private async Task HandleFirstReminderDue(User user, Loan loan) {
        bool reminderMailSentSuccessfully = await SendFirstReminderNotification(user, loan);
        
        if (reminderMailSentSuccessfully) {
            loan.TimesReminded++;
            _feeUpdateService.AddFirstReminderFee(user, loan);
        } else {
            // TODO: Save to MailTracker  
        }
    }
    
    private async Task HandleSecondReminderDue(User user, Loan loan) {
        bool reminderMailSentSuccessfully = await SendSecondReminderNotification(user, loan);
        
        if (reminderMailSentSuccessfully) {
            loan.TimesReminded++;
            _feeUpdateService.AddSecondReminderFee(user, loan);
        } else {
            // TODO: Save to MailTracker  
        }
    }

    private async Task<bool> SendFirstReminderNotification(User user, Loan loan) 
        => await TrySendMailAsync(MailTemplates.FirstReminder, new MailData(user, loan.Book.Title));
    
    private async Task<bool> SendSecondReminderNotification(User user, Loan loan) 
        => await TrySendMailAsync(MailTemplates.SecondReminder, new MailData(user, loan.Book.Title));
    
    private async Task<bool> SendAnnualFeeNotification(User user) 
        => await TrySendMailAsync(MailTemplates.AnnualFee, new MailData(user));
    
    private async Task<bool> TrySendMailAsync(MailTemplate template, MailData data) {
        string toAddress = data.User.Email;
        string body = template.Body(data);
        
        try {
            using MimeMessage message = new();
            message.From.Add(MailboxAddress.Parse(_smtpSettings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = template.Subject;
            message.Body = new TextPart("plain") { Text = body };

            using MailKit.Net.Smtp.SmtpClient client = new();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Successfully sent mail with subject {template.Subject} to {ToAddress}", template.Subject, toAddress);
            return true;
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to send mail with subject {template.Subject} to {ToAddress}.", template.Subject, toAddress);
            return false;
        }
    }
    
    /// <summary>
    /// Checks if an annual fee is due for the given user.
    /// This is true if one year has elapsed since the last annual fee or since the accession date of the user.
    /// </summary>
    private static bool AnnualFeeDue(User user) {
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
    
    /// <summary> Checks if a reminder fee for the given type has already been added to a loan. </summary>
    private static bool HasReminderFee(Loan loan, FeeType type) 
        => loan.Fees.Any(fee => fee.Type == type);
    
}