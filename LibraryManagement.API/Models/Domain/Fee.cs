using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class Fee {
    
    [MaxLength(50)]
    public required string Reason { get; set; }
    
    public decimal Amount { get; set; }

    public int TimesReminded { get; set; } = 0;
}