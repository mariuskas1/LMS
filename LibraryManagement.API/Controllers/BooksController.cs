using LibraryManagement.API.Data;
using LibraryManagement.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

public class BooksController: ControllerBase {
    private readonly LmsDbContext _dbContext;
    private readonly IBookRepository _bookRepository;

    public BooksController(LmsDbContext dbContext, IBookRepository bookRepository) {
        _dbContext = dbContext;
        _bookRepository = bookRepository;
    }
}