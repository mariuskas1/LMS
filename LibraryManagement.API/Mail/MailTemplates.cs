using LibraryManagement.API.Models.Domain;

namespace LibraryManagement.API.Mail;

public static class MailTemplates {
    
    public static readonly MailTemplate AnnualFee = new(
        "Jahresgebühr fällig",
        data => $"Hallo {data.User.FirstName}, die jährliche Bibliotheksgebühr wurde fällig."
    );

    public static readonly MailTemplate FirstReminder = new(
        "1. Mahnung",
        data => $"Hallo {data.User.FirstName}, folgende Medien sind seit 5 Tagen überfällig: {data.BookTitle}."
    );

    public static readonly MailTemplate SecondReminder = new(
        "2. Mahnung",
        data => $"Hallo {data.User.FirstName}, folgende Medien sind seit 10 Tagen überfällig: {data.BookTitle}."
    );
}

public record MailTemplate(string Subject, Func<MailData, string> Body);

public record MailData(User User,  string? BookTitle = null);