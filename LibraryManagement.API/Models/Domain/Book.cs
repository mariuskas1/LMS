using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class Book {
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }
    
    [MaxLength(100)]
    public string? Subtitle { get; set; }
    
    public required List<Author> Authors { get; set; }
    
    public int? PublishYear { get; set; }

    public bool IsBorrowed { get; set; } 
}