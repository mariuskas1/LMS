namespace LibraryManagement.API.Models.Domain;

public class Loan {
    public int Id { get; set; }

    public Guid BookId { get; set; }
    
    public required Book Book  { get; set; }
    
    public Guid UserId { get; set; }
    
    public required User User { get; set; }

    public DateTime LoanedAt { get; set; } 
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    
    public bool IsOverdue(DateTime now)
        => ReturnedAt == null && DueAt < now;
    
}