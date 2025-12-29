using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Models.Domain;

public class Author {
    public int Id { get; set; }
    
    [MaxLength(100)]
    public required string Name { get; set; }
}