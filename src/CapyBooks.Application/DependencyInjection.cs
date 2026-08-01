using System.Reflection;
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

        // Casos de uso e services de aplicação serão registrados aqui conforme forem implementados.

        return services;
    }
}
