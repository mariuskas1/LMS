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
        
        
        if (await CanUserLoan(userId)) {
            Loan newLoan = await CreateNewLoan(user, book);
            _logger.LogInformation("Loan {newLoan.Id} has been created.", newLoan);
        }
        
    }
    
    public void ExtendBookLoan(int bookId, int userId) {
        
    }

    private async Task<bool> CanUserLoan(int userId) {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null) {
            _logger.LogWarning("User {userId} not found", userId);
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

    private async Task<Loan> CreateNewLoan(User user, Book book) {
        Loan newLoan = new() {
            Book = book,
            BookId = book.Id,
            User = user,
            UserId = user.Id,

        };
            
        user.Loans.Add(newLoan);
        await _userRepository.UpdateAsync(user.Id, user);
        
        book.IsBorrowed = true;
        await _bookRepository.UpdateAsync(book.Id, book);
        
        await _loanRepository.CreateAsync(newLoan);

        return newLoan;
    }


}