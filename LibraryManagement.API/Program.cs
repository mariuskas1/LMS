using LibraryManagement.API.Data;
using LibraryManagement.API.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<LmsDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("LmsConnectionString")));

builder.Services.AddScoped<IBookRepository, SqlBookRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();