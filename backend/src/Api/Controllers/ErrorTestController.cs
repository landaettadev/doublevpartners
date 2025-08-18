using Common.Errors;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

/// <summary>
/// Controlador para probar el sistema de manejo de errores
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ErrorTestController : ControllerBase
{
    private readonly ILogger<ErrorTestController> _logger;

    public ErrorTestController(ILogger<ErrorTestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Prueba de excepción de validación
    /// </summary>
    [HttpGet("validation")]
    public IActionResult TestValidationError()
    {
        throw new Common.Errors.ValidationException(
            "Error de validación de prueba",
            "TestField",
            "Este es un error de validación de prueba para verificar el sistema.",
            new { TestData = "Datos de prueba" }
        );
    }

    /// <summary>
    /// Prueba de excepción de regla de negocio
    /// </summary>
    [HttpGet("business-rule")]
    public IActionResult TestBusinessRuleError()
    {
        throw new BusinessRuleException(
            "Error de regla de negocio de prueba",
            "TEST_BUSINESS_RULE",
            "Esta es una regla de negocio de prueba que no se puede cumplir.",
            new { RuleName = "TestRule", Value = 0 }
        );
    }

    /// <summary>
    /// Prueba de excepción de recurso no encontrado
    /// </summary>
    [HttpGet("not-found")]
    public IActionResult TestNotFoundError()
    {
        throw new NotFoundException(
            "Recurso de prueba no encontrado",
            "TestResource",
            999,
            "El recurso de prueba con ID 999 no existe en el sistema.",
            new { ResourceId = 999, ResourceType = "TestResource" }
        );
    }

    /// <summary>
    /// Prueba de excepción de conflicto
    /// </summary>
    [HttpGet("conflict")]
    public IActionResult TestConflictError()
    {
        throw new ConflictException(
            "Conflicto de prueba detectado",
            "DUPLICATE_TEST_RESOURCE",
            "Ya existe un recurso de prueba con el mismo identificador.",
            new { ResourceId = "TEST123", ExistingResource = "TestResource" }
        );
    }

    /// <summary>
    /// Prueba de excepción de base de datos
    /// </summary>
    [HttpGet("database")]
    public IActionResult TestDatabaseError()
    {
        throw new DatabaseException(
            "Error de base de datos de prueba",
            "TestDatabaseOperation",
            "Error SQL de prueba: Timeout expired",
            new { Operation = "TestOperation", Table = "TestTable" }
        );
    }

    /// <summary>
    /// Prueba de excepción de archivo
    /// </summary>
    [HttpGet("file-operation")]
    public IActionResult TestFileOperationError()
    {
        throw new FileOperationException(
            "Error de operación de archivo de prueba",
            "/test/path/file.txt",
            "TestFileOperation",
            "No se pudo realizar la operación de archivo de prueba.",
            new { FilePath = "/test/path/file.txt", Operation = "TestFileOperation" }
        );
    }

    /// <summary>
    /// Prueba de excepción de procesamiento de imagen
    /// </summary>
    [HttpGet("image-processing")]
    public IActionResult TestImageProcessingError()
    {
        throw new ImageProcessingException(
            "Error de procesamiento de imagen de prueba",
            "test/unknown",
            1024 * 1024 * 20, // 20MB
            "El archivo de imagen de prueba no es válido.",
            new { FileName = "test.unknown", ContentType = "test/unknown", FileSize = 1024 * 1024 * 20 }
        );
    }

    /// <summary>
    /// Prueba de excepción de configuración
    /// </summary>
    [HttpGet("configuration")]
    public IActionResult TestConfigurationError()
    {
        throw new ConfigurationException(
            "Error de configuración de prueba",
            "TestConfigKey",
            "La clave de configuración de prueba no se pudo leer.",
            new { ConfigKey = "TestConfigKey", Environment = "Test" }
        );
    }

    /// <summary>
    /// Prueba de excepción de servicio externo
    /// </summary>
    [HttpGet("external-service")]
    public IActionResult TestExternalServiceError()
    {
        throw new ExternalServiceException(
            "Error de servicio externo de prueba",
            "TestExternalService",
            "/api/test-endpoint",
            "El servicio externo de prueba no está disponible.",
            new { ServiceName = "TestExternalService", Endpoint = "/api/test-endpoint" }
        );
    }

    /// <summary>
    /// Prueba de excepción genérica
    /// </summary>
    [HttpGet("generic")]
    public IActionResult TestGenericError()
    {
        throw new Exception("Esta es una excepción genérica de prueba para verificar el manejo por defecto.");
    }

    /// <summary>
    /// Prueba de excepción de argumento
    /// </summary>
    [HttpGet("argument")]
    public IActionResult TestArgumentException()
    {
        throw new ArgumentException("Este es un argumento inválido de prueba", "testParam");
    }

    /// <summary>
    /// Prueba de excepción de operación inválida
    /// </summary>
    [HttpGet("invalid-operation")]
    public IActionResult TestInvalidOperationException()
    {
        throw new InvalidOperationException("Esta es una operación inválida de prueba");
    }

    /// <summary>
    /// Prueba de validación de modelo con datos inválidos
    /// </summary>
    [HttpPost("model-validation")]
    public IActionResult TestModelValidation([FromBody] TestModel model)
    {
        // Si llegamos aquí, significa que el ModelValidationMiddleware no funcionó
        return Ok("Modelo válido (esto no debería suceder si hay errores de validación)");
    }

    /// <summary>
    /// Prueba de logging de requests
    /// </summary>
    [HttpGet("logging")]
    public IActionResult TestLogging()
    {
        _logger.LogInformation("Prueba de logging de información");
        _logger.LogWarning("Prueba de logging de advertencia");
        _logger.LogError("Prueba de logging de error");
        
        return Ok(new
        {
            message = "Prueba de logging completada",
            timestamp = DateTime.UtcNow,
            logLevels = new[] { "Information", "Warning", "Error" }
        });
    }
}

/// <summary>
/// Modelo de prueba para validación
/// </summary>
public class TestModel
{
    [Required(ErrorMessage = "El campo Nombre es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo Email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "La edad debe estar entre 1 y 100")]
    public int Age { get; set; }

    [Required(ErrorMessage = "El campo Fecha es requerido")]
    public DateTime Date { get; set; }
}
