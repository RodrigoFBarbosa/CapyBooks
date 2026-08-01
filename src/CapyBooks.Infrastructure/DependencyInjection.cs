using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Authentication;
using CapyBooks.Infrastructure.ExternalServices;
using CapyBooks.Infrastructure.ExternalServices.GoogleBooks;
using CapyBooks.Infrastructure.ExternalServices.OpenLibrary;
using CapyBooks.Infrastructure.Persistence;
using CapyBooks.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CapyBooks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CapyBooksDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IBookshelfRepository, BookshelfRepository>();
        services.AddScoped<ICustomListRepository, CustomListRepository>();
        services.AddScoped<IReadingLinkRepository, ReadingLinkRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<GoogleAuthSettings>(configuration.GetSection("Authentication:Google"));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        services.AddHttpClient<OpenLibraryService>(client =>
        {
            client.BaseAddress = new Uri("https://openlibrary.org/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<GoogleBooksService>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/books/v1/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<IExternalBookSearchService, ExternalBookSearchService>();

        return services;
    }
}
