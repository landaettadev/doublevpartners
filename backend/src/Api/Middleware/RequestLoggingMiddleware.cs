using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Api.Middleware;

/// <summary>
/// Middleware para logging detallado de requests y responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log del request
            await LogRequestAsync(context, requestId);

            // Capturar el body de la respuesta
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Procesar el request
            await _next(context);

            // Log de la respuesta
            await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);

            // Copiar la respuesta al stream original
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error procesando request {RequestId} en {RequestPath}: {Message}", 
                requestId, context.Request.Path, ex.Message);
            
            // Restaurar el stream original
            context.Response.Body = originalBodyStream;
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        var request = context.Request;
        var logMessage = new StringBuilder();

        logMessage.AppendLine($"Request {requestId}:");
        logMessage.AppendLine($"  Method: {request.Method}");
        logMessage.AppendLine($"  Path: {request.Path}");
        logMessage.AppendLine($"  QueryString: {request.QueryString}");
        logMessage.AppendLine($"  ContentType: {request.ContentType}");
        logMessage.AppendLine($"  ContentLength: {request.ContentLength}");
        logMessage.AppendLine($"  UserAgent: {request.Headers["User-Agent"]}");
        logMessage.AppendLine($"  IP: {GetClientIpAddress(context)}");
        logMessage.AppendLine($"  Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");

        // Log de headers importantes
        var importantHeaders = new[] { "Authorization", "Accept", "Accept-Language", "Cache-Control" };
        foreach (var header in importantHeaders)
        {
            if (request.Headers.ContainsKey(header))
            {
                logMessage.AppendLine($"  {header}: {request.Headers[header]}");
            }
        }

        // Log del body si es JSON
        if (IsJsonRequest(request))
        {
            var body = await GetRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(body))
            {
                logMessage.AppendLine($"  Body: {body}");
            }
        }

        _logger.LogInformation(logMessage.ToString());
    }

    private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMs)
    {
        var response = context.Response;
        var logMessage = new StringBuilder();

        logMessage.AppendLine($"Response {requestId}:");
        logMessage.AppendLine($"  StatusCode: {response.StatusCode}");
        logMessage.AppendLine($"  ContentType: {response.ContentType}");
        logMessage.AppendLine($"  ContentLength: {response.ContentLength}");
        logMessage.AppendLine($"  ElapsedTime: {elapsedMs}ms");
        logMessage.AppendLine($"  Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");

        // Log de headers importantes de respuesta
        var importantResponseHeaders = new[] { "Cache-Control", "ETag", "Last-Modified" };
        foreach (var header in importantResponseHeaders)
        {
            if (response.Headers.ContainsKey(header))
            {
                logMessage.AppendLine($"  {header}: {response.Headers[header]}");
            }
        }

        // Log del body si es JSON y hay error
        if (IsErrorResponse(response.StatusCode) && IsJsonResponse(response))
        {
            var body = await GetResponseBodyAsync(context);
            if (!string.IsNullOrEmpty(body))
            {
                logMessage.AppendLine($"  ErrorBody: {body}");
            }
        }

        var logLevel = response.StatusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 300 and < 400 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, logMessage.ToString());
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.Split(',')[0].Trim();
        }

        var remoteIpAddress = context.Connection.RemoteIpAddress;
        return remoteIpAddress?.ToString() ?? "Unknown";
    }

    private bool IsJsonRequest(HttpRequest request)
    {
        return request.ContentType?.Contains("application/json") == true ||
               request.ContentType?.Contains("text/json") == true;
    }

    private bool IsJsonResponse(HttpResponse response)
    {
        return response.ContentType?.Contains("application/json") == true ||
               response.ContentType?.Contains("text/json") == true;
    }

    private bool IsErrorResponse(int statusCode)
    {
        return statusCode >= 400;
    }

    private async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();
            var buffer = new byte[request.ContentLength ?? 0];
            var bytesRead = await request.Body.ReadAsync(buffer, 0, buffer.Length);
            request.Body.Position = 0;

            if (bytesRead > 0)
            {
                var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return body.Length > 1000 ? body[..1000] + "..." : body;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el body del request");
        }

        return string.Empty;
    }

    private async Task<string> GetResponseBodyAsync(HttpContext context)
    {
        try
        {
            if (context.Response.Body is MemoryStream memoryStream)
            {
                memoryStream.Position = 0;
                var buffer = new byte[memoryStream.Length];
                var bytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length);
                memoryStream.Position = 0;

                if (bytesRead > 0)
                {
                    var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    return body.Length > 1000 ? body[..1000] + "..." : body;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el body de la respuesta");
        }

        return string.Empty;
    }
}
