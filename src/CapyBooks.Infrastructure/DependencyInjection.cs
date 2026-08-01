using CapyBooks.Infrastructure.Persistence;
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
            options.UseNpgsql(connectionString));

        // Repositórios (IUserRepository, IBookRepository, etc.) e serviços externos
        // (Open Library, Google Books) serão registrados aqui conforme forem implementados.

        return services;
    }
}
