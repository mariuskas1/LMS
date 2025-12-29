using LibraryManagement.API.Clients;
using LibraryManagement.API.Data;
using LibraryManagement.API.Mappings;
using LibraryManagement.API.Repositories;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<LmsDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("LmsConnectionString")));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<OpenLibraryClient>();
builder.Services.AddAutoMapper( cfg => { },
    typeof(AutomapperProfiles).Assembly);

WebApplication app = builder.Build();



using (IServiceScope scope = app.Services.CreateScope()) {
    DatabaseSeeder seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedDatabaseAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();