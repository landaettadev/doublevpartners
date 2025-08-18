using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api.Middleware;

/// <summary>
/// Middleware para validación automática de modelos
/// </summary>
public class ModelValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ModelValidationMiddleware> _logger;

    public ModelValidationMiddleware(RequestDelegate next, ILogger<ModelValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Por ahora, este middleware solo pasa la solicitud al siguiente
        // La validación de modelos se maneja automáticamente por ASP.NET Core
        await _next(context);
    }
}
