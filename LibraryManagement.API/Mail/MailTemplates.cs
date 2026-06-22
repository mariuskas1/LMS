namespace LibraryManagement.API.Mail;

public static class MailTemplates {
    
    public static class AnnualFee {
        public const string Subject = "Jahresgebühr fällig";
        public static string Body(string firstName) =>
            $"Hallo {firstName}, die jährliche Bibliotheksgebühr wurde fällig.";
    }

    public static class FirstReminder {
        public const string Subject = "1. Mahnung: Überfälliges Buch";
        public static string Body(string firstName) =>
            $"Hallo {firstName}, Ihr Buch ist seit 5 Tagen überfällig. Bitte geben Sie es baldmöglichst zurück.";
    }

    public static class SecondReminder {
        public const string Subject = "2. Mahnung: Überfälliges Buch";
        public static string Body(string firstName) =>
            $"Hallo {firstName}, Ihr Buch ist seit 10 Tagen überfällig. Eine Mahngebühr wurde Ihrem Konto hinzugefügt.";
    }
}

public record MailTemplate(string Subject, Func<string, string> Body);