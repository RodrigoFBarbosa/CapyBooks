using CapyBooks.Domain.Interfaces;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Serviços externos (Open Library, Google Books) serão registrados aqui
        // conforme forem implementados.

        return services;
    }
}
