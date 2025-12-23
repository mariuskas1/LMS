namespace LibraryManagement.API.Models.Domain;

public class Loan {
    public Guid Id { get; set; }

    public Guid BookId { get; set; }
    public Guid UserId { get; set; }

    public DateTime LoanedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime DueAtUtc { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
    
}