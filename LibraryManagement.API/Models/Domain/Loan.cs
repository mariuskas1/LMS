namespace LibraryManagement.API.Models.Domain;

public class Loan {
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book  { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
    
    public DateTime LoanedAt { get; set; } = DateTime.Now;
    public DateTime DueAt { get; set; }  = DateTime.Now + TimeSpan.FromDays(28);
    public DateTime? ReturnedAt { get; set; }
    public int TimesExtended { get; set; } 
    public int TimesReminded { get; set; }
    
    public DateTime LastRemindedAt { get; set; } 
    
    public List<Fee> Fees { get; set; } = [];
    
    public decimal FeeAmount => Fees.Sum(f => f.Amount);
    
    public bool IsOverdue(DateTime now)
        => ReturnedAt == null && DueAt < now;
    
}