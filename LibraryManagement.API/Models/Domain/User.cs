namespace LibraryManagement.API.Models.Domain;

public class User {
    public Guid Id { get; set; }

    public string Email { get; set; }
    public string DisplayName { get; set; }
}