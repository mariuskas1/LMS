namespace LibraryManagement.API.Models.Domain;

public class Book {
    public Guid Id { get; set; }

    public string Title { get; set; }
    public string? Subtitle { get; set; }
    public string Author { get; set; }
    public int? PublishYear { get; set; }
    public bool IsBorrowed { get; set; }
}