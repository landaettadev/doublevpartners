using System.Net;
using System.Text.Json;
using Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        var errorResponse = new ErrorResponse();

        // Determinar el tipo de excepción y configurar la respuesta
        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = CreateValidationErrorResponse(validationEx);
                break;

            case BusinessRuleException businessEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = CreateBusinessRuleErrorResponse(businessEx);
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = CreateNotFoundErrorResponse(notFoundEx);
                break;

            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = CreateConflictErrorResponse(conflictEx);
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = CreateUnauthorizedErrorResponse(unauthorizedEx);
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse = CreateForbiddenErrorResponse(forbiddenEx);
                break;

            case DatabaseException dbEx:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = CreateDatabaseErrorResponse(dbEx);
                break;

            case ExternalServiceException externalEx:
                response.StatusCode = (int)HttpStatusCode.BadGateway;
                errorResponse = CreateExternalServiceErrorResponse(externalEx);
                break;

            case ConfigurationException configEx:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = CreateConfigurationErrorResponse(configEx);
                break;

            case FileOperationException fileEx:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = CreateFileOperationErrorResponse(fileEx);
                break;

            case ImageProcessingException imageEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = CreateImageProcessingErrorResponse(imageEx);
                break;

            case Common.Errors.ApplicationException appEx:
                response.StatusCode = appEx.StatusCode;
                errorResponse = CreateApplicationErrorResponse(appEx);
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = CreateArgumentExceptionResponse(argEx);
                break;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = CreateInvalidOperationErrorResponse(invalidOpEx);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = CreateGenericErrorResponse(exception);
                break;
        }

        // Logging del error
        LogException(exception, context, errorResponse);

        // Configurar headers de respuesta
        response.ContentType = "application/json";

        // Serializar y enviar respuesta
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await response.WriteAsync(jsonResponse);
    }

    private ErrorResponse CreateValidationErrorResponse(ValidationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = ex.ValidationErrors.Select(v => new ErrorDetail
            {
                Field = v.FieldName,
                Message = v.ErrorMessage,
                Code = v.ErrorCode,
                AttemptedValue = v.AttemptedValue,
                Suggestion = v.Suggestion
            }).ToList(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateBusinessRuleErrorResponse(BusinessRuleException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "BusinessRule",
                    Message = ex.BusinessRule,
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateNotFoundErrorResponse(NotFoundException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Resource",
                    Message = $"No se encontró {ex.ResourceType} con ID: {ex.ResourceId}",
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateConflictErrorResponse(ConflictException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Conflict",
                    Message = ex.ConflictType,
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateUnauthorizedErrorResponse(UnauthorizedException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Authorization",
                    Message = ex.Reason,
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateForbiddenErrorResponse(ForbiddenException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Permission",
                    Message = $"Se requiere permiso: {ex.RequiredPermission}",
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateDatabaseErrorResponse(DatabaseException ex)
    {
        var details = new List<ErrorDetail>
        {
            new()
            {
                Field = "Operation",
                Message = ex.Operation,
                Code = ex.ErrorCode
            }
        };

        if (!string.IsNullOrEmpty(ex.SqlError))
        {
            details.Add(new ErrorDetail
            {
                Field = "SqlError",
                Message = ex.SqlError,
                Code = "SQL_ERROR"
            });
        }

        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = details,
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateExternalServiceErrorResponse(ExternalServiceException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Service",
                    Message = $"{ex.ServiceName}: {ex.Endpoint}",
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateConfigurationErrorResponse(ConfigurationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Configuration",
                    Message = $"Clave: {ex.ConfigurationKey}",
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateFileOperationErrorResponse(FileOperationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "FileOperation",
                    Message = $"{ex.Operation} en {ex.FilePath}",
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateImageProcessingErrorResponse(ImageProcessingException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "ImageProcessing",
                    Message = $"Formato: {ex.ImageFormat}, Tamaño: {ex.FileSize} bytes",
                    Code = ex.ErrorCode
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateApplicationErrorResponse(Common.Errors.ApplicationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = ex.AdditionalData != null ? new List<ErrorDetail>
            {
                new()
                {
                    Field = "AdditionalData",
                    Message = ex.AdditionalData.ToString(),
                    Code = ex.ErrorCode
                }
            } : new List<ErrorDetail>(),
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl(ex.ErrorCode)
        };
    }

    private ErrorResponse CreateArgumentExceptionResponse(ArgumentException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = "ARGUMENT_ERROR",
            Message = "Error en los argumentos proporcionados",
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = ex.ParamName ?? "Unknown",
                    Message = ex.Message,
                    Code = "ARGUMENT_ERROR"
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl("ARGUMENT_ERROR")
        };
    }

    private ErrorResponse CreateInvalidOperationErrorResponse(InvalidOperationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = "INVALID_OPERATION",
            Message = "Operación no válida en el estado actual",
            Details = new List<ErrorDetail>
            {
                new()
                {
                    Field = "Operation",
                    Message = ex.Message,
                    Code = "INVALID_OPERATION"
                }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl("INVALID_OPERATION")
        };
    }

    private ErrorResponse CreateGenericErrorResponse(Exception ex)
    {
        var details = new List<ErrorDetail>();

        if (_environment.IsDevelopment())
        {
            details.Add(new ErrorDetail
            {
                Field = "StackTrace",
                Message = ex.StackTrace ?? "No disponible",
                Code = "STACK_TRACE"
            });

            details.Add(new ErrorDetail
            {
                Field = "InnerException",
                Message = ex.InnerException?.Message ?? "No disponible",
                Code = "INNER_EXCEPTION"
            });
        }

        return new ErrorResponse
        {
            ErrorCode = "INTERNAL_SERVER_ERROR",
            Message = _environment.IsDevelopment() ? ex.Message : "Ha ocurrido un error interno en el servidor",
            Details = details,
            Timestamp = DateTime.UtcNow,
            TraceId = GetTraceId(),
            HelpUrl = GetHelpUrl("INTERNAL_SERVER_ERROR")
        };
    }

    private void LogException(Exception exception, HttpContext context, ErrorResponse errorResponse)
    {
        var logLevel = exception switch
        {
            ValidationException => LogLevel.Warning,
            BusinessRuleException => LogLevel.Warning,
            NotFoundException => LogLevel.Information,
            ConflictException => LogLevel.Warning,
            UnauthorizedException => LogLevel.Warning,
            ForbiddenException => LogLevel.Warning,
            DatabaseException => LogLevel.Error,
            ExternalServiceException => LogLevel.Error,
            ConfigurationException => LogLevel.Critical,
            FileOperationException => LogLevel.Error,
            ImageProcessingException => LogLevel.Warning,
            Common.Errors.ApplicationException => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(logLevel, exception, 
            "Error {ErrorCode} en {RequestPath}: {Message}. TraceId: {TraceId}", 
            errorResponse.ErrorCode, 
            context.Request.Path, 
            errorResponse.Message, 
            errorResponse.TraceId);
    }

    private string GetTraceId()
    {
        return System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }

    private string GetHelpUrl(string errorCode)
    {
        return $"https://api.doublevpartners.com/docs/errors/{errorCode.ToLowerInvariant()}";
    }
}

/// <summary>
/// Respuesta de error estructurada
/// </summary>
public class ErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetail> Details { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string HelpUrl { get; set; } = string.Empty;
}

/// <summary>
/// Detalle específico de un error
/// </summary>
public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public object? AttemptedValue { get; set; }
    public string? Suggestion { get; set; }
}
