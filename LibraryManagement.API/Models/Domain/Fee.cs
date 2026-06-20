using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class Fee {
    public FeeType Type { get; set; } = FeeType.Standard;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [MaxLength(50)]
    public required string Reason { get; set; }
    public decimal Amount { get; set; }

    public int TimesReminded { get; set; } = 0;

    public static Fee CreateOverdueFee(decimal amount, int loanId) 
        => new() {
                Amount = amount,
                Reason = $"Overdue fee for the loan {loanId}."
        };

    public static Fee CreateFirstReminderFee(int loanId)
        => new() {
            Amount = 1.0m,
            Reason = $"First reminder fee for the overdue loan {loanId}.",
            Type = FeeType.FirstReminder
        };

    public static Fee CreateSecondReminderFee(int loanId)
        => new() {
            Amount = 3.0m,
            Reason = $"Second reminder fee for the overdue loan {loanId}.",
            Type = FeeType.SecondReminder
        };
}