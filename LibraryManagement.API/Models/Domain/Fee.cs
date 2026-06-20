using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class Fee {
    public FeeType Type { get; set; } = FeeType.Standard;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [MaxLength(50)]
    public required string Description { get; set; }
    public decimal Amount { get; set; }

    public int TimesReminded { get; set; } = 0;

    /// <summary> Returns a new overdue fee with the given amount and loanId. </summary>
    public static Fee CreateOverdueFee(decimal amount, int loanId) 
        => new() {
                Amount = amount,
                Description = $"Overdue fee for the loan {loanId}."
        };

    /// <summary> Returns a new first reminder fee object with the given loanId. </summary>
    public static Fee CreateFirstReminderFee(int loanId)
        => new() {
            Amount = 1.0m,
            Description = $"First reminder fee for the overdue loan {loanId}.",
            Type = FeeType.FirstReminder
        };

    /// <summary> Returns a new second reminder fee with the given loanId. </summary>
    public static Fee CreateSecondReminderFee(int loanId)
        => new() {
            Amount = 3.0m,
            Description = $"Second reminder fee for the overdue loan {loanId}.",
            Type = FeeType.SecondReminder
        };

    /// <summary> Returns a new annual fee for the given year. </summary>
    public static Fee CreateAnnualFee(int year)
        => new() {
            Amount = 30,
            Description = $"Annual fee for year {year}",
            Type = FeeType.Annual
        };
}