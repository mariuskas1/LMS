using LibraryManagement.API.Data;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BooksController: ControllerBase {
    private readonly LmsDbContext _dbContext;
    private readonly IBookRepository _bookRepository;

    public BooksController(LmsDbContext dbContext, IBookRepository bookRepository) {
        _dbContext = dbContext;
        _bookRepository = bookRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Book book) {
        Book bookDomainModel = await _bookRepository.CreateAsync(book);
        return Ok();
    }
    
}