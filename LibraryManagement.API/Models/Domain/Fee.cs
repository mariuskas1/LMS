using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class Fee {
    public FeeType Type { get; set; } = FeeType.Standard;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [MaxLength(50)]
    public required string Reason { get; set; }
    public decimal Amount { get; set; }

    public int TimesReminded { get; set; } = 0;
}