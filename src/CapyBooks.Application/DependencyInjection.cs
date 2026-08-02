using System.Reflection;
using CapyBooks.Application.Interfaces;
using CapyBooks.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CapyBooks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(cfg => { }, assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IBookshelfService, BookshelfService>();
        services.AddScoped<ICustomListService, CustomListService>();

        // Demais casos de uso e services de aplicação serão registrados aqui conforme forem implementados.

        return services;
    }
}
