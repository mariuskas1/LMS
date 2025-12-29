using LibraryManagement.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Data;

public class LmsDbContext : DbContext {
    
    public LmsDbContext(DbContextOptions options) : base(options) {
        
    }

    public DbSet<Book> Books { get; set; }
    
    public DbSet<Loan> Loans { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<Author> Authors { get; set; }
}