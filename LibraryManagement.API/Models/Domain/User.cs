using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class User {
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Email { get; set; }
    
    [MaxLength(100)]
    public required string Name { get; set; }
    
    [MaxLength(100)]
    public required string FirstName { get; set; }
    
    public DateTime AccessionDate { get; set; } = DateTime.Now;
    
    public DateTime AnnualFeeDue => AccessionDate.AddYears(1);

    public List<Fee> OutstandingFees { get; set; } = [];
    
    public decimal OutstandingFeesTotal => OutstandingFees?.Sum(fee => fee.Amount) ?? 0;

    public List<Loan> Loans { get; set; } = [];

}