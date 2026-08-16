using Bookstore.Api.Configuration;
using Bookstore.Api.Data;
using Bookstore.Api.ErrorHandling;
using Bookstore.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Bookstore")
    ?? throw new InvalidOperationException("Connection string 'Bookstore' is not configured.");
var authority = builder.Configuration["Authentication:Authority"] ?? "http://localhost:5277";
var clientSecret = builder.Configuration["Authentication:ClientCredentialsSecret"]
    ?? throw new InvalidOperationException("Client credentials secret is not configured.");

builder.Services.AddDbContext<BookstoreDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddBookstoreAuth(authority, clientSecret);
builder.Services.AddControllers();
builder.Services.AddAntiforgery();
builder.Services.AddBookstoreSwagger(authority);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API v1");
        options.DisplayRequestDuration();
        options.OAuthClientId(AuthExtensions.SwaggerClientId);
        options.OAuthAppName("Bookstore API Swagger");
    });

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program;
