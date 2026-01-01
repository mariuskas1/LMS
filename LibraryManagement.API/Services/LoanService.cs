using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;

namespace LibraryManagement.API.Services;

/// <summary> Holds the logic to loan, extend or return a book. </summary>
public class LoanService {
    private readonly ILoanRepository _loanRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<LoanService> _logger;

    /// <summary> Creates a new instance. </summary>
    public LoanService (ILoanRepository loanRepository, IUserRepository userRepository, IBookRepository bookRepository, ILogger<LoanService> logger) {
        _loanRepository = loanRepository;
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }

    /// <summary> Calls the necessary functions to create a new book loan and to update the database accordingly. </summary>
    /// <param name="bookId">The book to be loaned. </param>
    /// <param name="userId"> The user the loan is created for. </param>
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
        
            // Update book
            book.IsBorrowed = true;
            await _bookRepository.UpdateAsync(book.Id, book);
        
            // Create new loan
            await _loanRepository.CreateAsync(newLoan);
            _logger.LogInformation("New loan {newLoan.Id} has been created.", newLoan);
        }
    }
    
    
    /// <summary> Calls the necessary functions to extend a book loan about 28 days. </summary>
    /// <param name="bookId">The id of the book which loan should be extended. </param>
    /// <param name="userId">The id of the user the loan should be extended for. </param>
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

    /// <summary> Calls the necessary functions to return a book and update the database accordingly. </summary>
    /// <param name="bookId">The id of the book that is returned. </param>
    public async Task ReturnBook(int bookId) {
        Book? book = await _bookRepository.GetByIdAsync(bookId);
        List<Loan> allLoans = await _loanRepository.GetAllAsync();
        Loan? targetedLoan = allLoans.FirstOrDefault(loan => loan.BookId == bookId);
        if (targetedLoan == null || book == null) return;
        
        targetedLoan.ReturnedAt = DateTime.Now;
        book.IsBorrowed = false;
    }

    /// <summary> Checks if a user is able to loan a book or extend an existing loan. The requirements for both are the same: A user can't have any fees or overdue loans.
    /// </summary>
    /// <param name="userId"> The id of the user whose account is checked. </param>
    private async Task<bool> CanUserLoan(int userId) {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null) {
            _logger.LogWarning("User with id {userId} not found", userId);
            return false;
        }

        if (user.OutstandingFeesTotal != 0) {
            _logger.LogInformation("User {userId} has outstanding fees ", userId);
            return false;
        }

        if (user.Loans.Any(loan => loan.IsOverdue(DateTime.Now))) {
            _logger.LogInformation("User {userId} has overdue loan ", userId);
            return false;
        }
        
        return true;
    }

    /// <summary> Creates a new loan object. </summary>
    /// <param name="user"> The user for whom the loan is created. </param>
    /// <param name="book"> The book that is loaned.</param>
    /// <returns> Returns the new loan object. </returns>
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