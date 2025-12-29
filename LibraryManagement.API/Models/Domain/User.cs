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
    
    public decimal OutstandingFees { get; set; }
    
    public List<Loan>? Loans { get; set; }
}