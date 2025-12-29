using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Services;

public class LoanService {
    private readonly ILoanRepository _loanRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<LoanService> _logger;

    public LoanService (
        ILoanRepository loanRepository, 
        IUserRepository userRepository, 
        IBookRepository bookRepository,
        ILogger<LoanService> logger
    ) {
        _loanRepository = loanRepository;
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task LoanBook(int bookId, int userId) {
        User? user = await _userRepository.GetByIdAsync(userId);
        Book? book = await _bookRepository.GetByIdAsync(bookId);

        if (user == null) {
            _logger.LogWarning("User {userId} not found", userId);
            return;
        }

        if (book == null) {
            _logger.LogWarning("Book {bookId} not found", bookId);
            return;
        }
        
        
        if (await CanUserLoan(userId) && !book.IsBorrowed) {
            Loan newLoan = GetNewLoan(user, book);
            
            // Update loans
            user.Loans.Add(newLoan);
            await _userRepository.UpdateAsync(user.Id, user);
        
            // Update books
            book.IsBorrowed = true;
            await _bookRepository.UpdateAsync(book.Id, book);
        
            // Create new loan
            await _loanRepository.CreateAsync(newLoan);
            _logger.LogInformation("New loan {newLoan.Id} has been created.", newLoan);
        }
    }
    
    public async Task ExtendBookLoan(int bookId, int userId) {
        List<Loan> allLoans = await _loanRepository.GetAllAsync();
        Loan? targetedLoan = allLoans.FirstOrDefault(loan => loan.BookId == bookId && loan.UserId == userId);
        if (targetedLoan == null) return;

        if (targetedLoan.TimesExtended >= 3) {
            _logger.LogInformation("The loan for the book {loan.BookId} cannot be extended. It has already been extended three times.", targetedLoan);
            return;
        }
        
        if (await CanUserLoan(userId)) {
            targetedLoan.DueAt += TimeSpan.FromDays(28);
            targetedLoan.TimesExtended++;
            
            await _loanRepository.UpdateAsync(targetedLoan.Id, targetedLoan);
            _logger.LogInformation("Loan {targetedLoan.Id} has been extended.", targetedLoan);
        }
    }

    public async Task ReturnBook(int bookId) {
        Book? book = await _bookRepository.GetByIdAsync(bookId);
        List<Loan> allLoans = await _loanRepository.GetAllAsync();
        Loan? targetedLoan = allLoans.FirstOrDefault(loan => loan.BookId == bookId);
        if (targetedLoan == null || book == null) return;
        
        targetedLoan.ReturnedAt = DateTime.Now;
        book.IsBorrowed = false;
    }

    private async Task<bool> CanUserLoan(int userId) {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null) {
            _logger.LogWarning("User with id {userId} not found", userId);
            return false;
        }

        if (user.OutstandingFees != 0) {
            _logger.LogInformation("User {userId} has outstanding fees ", userId);
            return false;
        }

        if (user.Loans.Any(loan => loan.IsOverdue(DateTime.Now))) {
            _logger.LogInformation("User {userId} has overdue loan ", userId);
            return false;
        }
        
        return true;
    }

    private static Loan GetNewLoan(User user, Book book) {
        Loan newLoan = new() {
            Book = book,
            BookId = book.Id,
            User = user,
            UserId = user.Id,
        };

        return newLoan;
    }
    
}