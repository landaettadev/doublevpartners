using Application.Interfaces;
using Application.Services;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Extensiones para la configuración de servicios de la aplicación
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Agrega todos los servicios de la aplicación
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Logger personalizado
        services.AddSingleton<ILogger, ConsoleLogger>();
        
        // Servicios de aplicación
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();

        // Configuración de JWT
        services.Configure<Common.Models.JwtSettings>(configuration.GetSection("Jwt"));

        return services;
    }
}
