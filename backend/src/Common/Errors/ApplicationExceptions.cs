namespace Common.Errors;

/// <summary>
/// Excepción base para errores de dominio de la aplicación
/// </summary>
public abstract class ApplicationException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public string UserMessage { get; }
    public object? AdditionalData { get; }

    protected ApplicationException(string message, string errorCode, int statusCode, string userMessage = "", object? additionalData = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        UserMessage = string.IsNullOrEmpty(userMessage) ? message : userMessage;
        AdditionalData = additionalData;
    }
}

/// <summary>
/// Excepción para errores de validación de datos
/// </summary>
public class ValidationException : ApplicationException
{
    public IReadOnlyList<ValidationError> ValidationErrors { get; }

    public ValidationException(string message, IReadOnlyList<ValidationError> validationErrors, object? additionalData = null)
        : base(message, "VALIDATION_ERROR", 400, "Los datos proporcionados no son válidos", additionalData)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message, string fieldName, string errorMessage, object? additionalData = null)
        : base(message, "VALIDATION_ERROR", 400, "Los datos proporcionados no son válidos", additionalData)
    {
        ValidationErrors = new List<ValidationError> { new(fieldName, errorMessage) };
    }
}

/// <summary>
/// Excepción para errores de negocio
/// </summary>
public class BusinessRuleException : ApplicationException
{
    public string BusinessRule { get; }

    public BusinessRuleException(string message, string businessRule, string userMessage = "", object? additionalData = null)
        : base(message, "BUSINESS_RULE_VIOLATION", 422, userMessage, additionalData)
    {
        BusinessRule = businessRule;
    }
}

/// <summary>
/// Excepción para recursos no encontrados
/// </summary>
public class NotFoundException : ApplicationException
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public NotFoundException(string message, string resourceType, object resourceId, string userMessage = "", object? additionalData = null)
        : base(message, "RESOURCE_NOT_FOUND", 404, userMessage, additionalData)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Excepción para conflictos de datos
/// </summary>
public class ConflictException : ApplicationException
{
    public string ConflictType { get; }

    public ConflictException(string message, string conflictType, string userMessage = "", object? additionalData = null)
        : base(message, "CONFLICT", 409, userMessage, additionalData)
    {
        ConflictType = conflictType;
    }
}

/// <summary>
/// Excepción para errores de autenticación
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public string Reason { get; }

    public UnauthorizedException(string message, string reason, string userMessage = "", object? additionalData = null)
        : base(message, "UNAUTHORIZED", 401, userMessage, additionalData)
    {
        Reason = reason;
    }
}

/// <summary>
/// Excepción para errores de autorización
/// </summary>
public class ForbiddenException : ApplicationException
{
    public string RequiredPermission { get; }

    public ForbiddenException(string message, string requiredPermission, string userMessage = "", object? additionalData = null)
        : base(message, "FORBIDDEN", 403, userMessage, additionalData)
    {
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Excepción para errores de base de datos
/// </summary>
public class DatabaseException : ApplicationException
{
    public string Operation { get; }
    public string? SqlError { get; }

    public DatabaseException(string message, string operation, string? sqlError = null, object? additionalData = null)
        : base(message, "DATABASE_ERROR", 500, "Error en la base de datos", additionalData)
    {
        Operation = operation;
        SqlError = sqlError;
    }
}

/// <summary>
/// Excepción para errores de integración externa
/// </summary>
public class ExternalServiceException : ApplicationException
{
    public string ServiceName { get; }
    public string Endpoint { get; }

    public ExternalServiceException(string message, string serviceName, string endpoint, string userMessage = "", object? additionalData = null)
        : base(message, "EXTERNAL_SERVICE_ERROR", 502, userMessage, additionalData)
    {
        ServiceName = serviceName;
        Endpoint = endpoint;
    }
}

/// <summary>
/// Excepción para errores de configuración
/// </summary>
public class ConfigurationException : ApplicationException
{
    public string ConfigurationKey { get; }

    public ConfigurationException(string message, string configurationKey, string userMessage = "", object? additionalData = null)
        : base(message, "CONFIGURATION_ERROR", 500, userMessage, additionalData)
    {
        ConfigurationKey = configurationKey;
    }
}

/// <summary>
/// Excepción para errores de archivo
/// </summary>
public class FileOperationException : ApplicationException
{
    public string FilePath { get; }
    public string Operation { get; }

    public FileOperationException(string message, string filePath, string operation, string userMessage = "", object? additionalData = null)
        : base(message, "FILE_OPERATION_ERROR", 500, userMessage, additionalData)
    {
        FilePath = filePath;
        Operation = operation;
    }
}

/// <summary>
/// Excepción para errores de imagen
/// </summary>
public class ImageProcessingException : ApplicationException
{
    public string ImageFormat { get; }
    public long FileSize { get; }

    public ImageProcessingException(string message, string imageFormat, long fileSize, string userMessage = "", object? additionalData = null)
        : base(message, "IMAGE_PROCESSING_ERROR", 400, userMessage, additionalData)
    {
        ImageFormat = imageFormat;
        FileSize = fileSize;
    }
}
