using Common.Errors;

namespace Application.Services;

/// <summary>
/// Servicio centralizado para validaciones comunes
/// </summary>
public class ValidationService
{
    /// <summary>
    /// Valida que un ID sea válido
    /// </summary>
    public static void ValidateId(int id, string fieldName)
    {
        if (id <= 0)
        {
            throw new ValidationException(
                $"El {fieldName} debe ser mayor a 0",
                fieldName,
                $"El {fieldName} debe ser un valor positivo.",
                new { Id = id, MinId = 1 }
            );
        }
    }

    /// <summary>
    /// Valida que una cadena no esté vacía
    /// </summary>
    public static void ValidateRequiredString(string value, string fieldName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(
                $"El {displayName} es obligatorio",
                fieldName,
                $"El {displayName} es obligatorio y no puede estar vacío.",
                new { FieldName = fieldName, Value = value }
            );
        }
    }

    /// <summary>
    /// Valida la longitud de una cadena
    /// </summary>
    public static void ValidateStringLength(string value, string fieldName, string displayName, int minLength, int maxLength)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            throw new ValidationException(
                $"El {displayName} debe tener entre {minLength} y {maxLength} caracteres",
                fieldName,
                $"El {displayName} debe tener entre {minLength} y {maxLength} caracteres. Longitud actual: {value.Length}",
                new { FieldName = fieldName, Value = value, MinLength = minLength, MaxLength = maxLength, CurrentLength = value.Length }
            );
        }
    }

    /// <summary>
    /// Valida que un precio sea válido
    /// </summary>
    public static void ValidatePrice(decimal price, string fieldName, string displayName)
    {
        if (price <= 0)
        {
            throw new ValidationException(
                $"El {displayName} debe ser mayor a 0",
                fieldName,
                $"El {displayName} debe ser un valor positivo mayor a cero.",
                new { FieldName = fieldName, Price = price, MinPrice = 0.01m }
            );
        }
    }

    /// <summary>
    /// Valida que una cantidad sea válida
    /// </summary>
    public static void ValidateQuantity(int quantity, string fieldName, string displayName, int minQuantity = 1, int maxQuantity = 1000)
    {
        if (quantity < minQuantity || quantity > maxQuantity)
        {
            throw new ValidationException(
                $"La {displayName} debe estar entre {minQuantity} y {maxQuantity}",
                fieldName,
                $"La {displayName} debe estar entre {minQuantity} y {maxQuantity}. Valor actual: {quantity}",
                new { FieldName = fieldName, Quantity = quantity, MinQuantity = minQuantity, MaxQuantity = maxQuantity }
            );
        }
    }

    /// <summary>
    /// Valida que una fecha sea válida
    /// </summary>
    public static void ValidateDate(DateTime date, string fieldName, string displayName, bool allowFuture = false, int maxYearsInPast = 10)
    {
        if (!allowFuture && date > DateTime.Today)
        {
            throw new ValidationException(
                $"La {displayName} no puede ser futura",
                fieldName,
                $"La {displayName} no puede ser futura. Use una fecha actual o pasada.",
                new { FieldName = fieldName, Date = date, Today = DateTime.Today }
            );
        }

        if (date < DateTime.Today.AddYears(-maxYearsInPast))
        {
            throw new ValidationException(
                $"La {displayName} no puede ser muy antigua",
                fieldName,
                $"La {displayName} no puede ser anterior a {maxYearsInPast} años.",
                new { FieldName = fieldName, Date = date, MaxYearsInPast = maxYearsInPast }
            );
        }
    }

    /// <summary>
    /// Valida que un email tenga formato válido
    /// </summary>
    public static void ValidateEmail(string email, string fieldName = "Email")
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException(
                "El email es obligatorio",
                fieldName,
                "El email es obligatorio y no puede estar vacío.",
                new { FieldName = fieldName, Email = email }
            );
        }

        try
        {
            var emailAddress = new System.Net.Mail.MailAddress(email);
            if (emailAddress.Address != email)
            {
                throw new ValidationException(
                    "El formato del email no es válido",
                    fieldName,
                    "El formato del email no es válido. Use formato: usuario@dominio.com",
                    new { FieldName = fieldName, Email = email }
                );
            }
        }
        catch
        {
            throw new ValidationException(
                "El formato del email no es válido",
                fieldName,
                "El formato del email no es válido. Use formato: usuario@dominio.com",
                new { FieldName = fieldName, Email = email }
            );
        }
    }

    /// <summary>
    /// Valida que un número de teléfono tenga formato válido
    /// </summary>
    public static void ValidatePhoneNumber(string phone, string fieldName = "Phone")
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return; // El teléfono es opcional
        }

        // Validar formato básico: +57 300 123 4567 o 300 123 4567
        var phoneRegex = new System.Text.RegularExpressions.Regex(@"^(\+57\s?)?[0-9]{3}\s?[0-9]{3}\s?[0-9]{4}$");
        if (!phoneRegex.IsMatch(phone))
        {
            throw new ValidationException(
                "El formato del número de teléfono no es válido",
                fieldName,
                "El formato del número de teléfono no es válido. Use formato: +57 300 123 4567 o 300 123 4567",
                new { FieldName = fieldName, Phone = phone }
            );
        }
    }

    /// <summary>
    /// Valida que un término de búsqueda sea válido
    /// </summary>
    public static void ValidateSearchTerm(string searchTerm, string fieldName = "SearchTerm", int minLength = 2, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ValidationException(
                "El término de búsqueda no puede estar vacío",
                fieldName,
                "El término de búsqueda es obligatorio y debe contener al menos un carácter.",
                new { FieldName = fieldName, SearchTerm = searchTerm, MinLength = minLength }
            );
        }

        if (searchTerm.Length < minLength)
        {
            throw new ValidationException(
                $"El término de búsqueda debe tener al menos {minLength} caracteres",
                fieldName,
                $"Para obtener resultados relevantes, el término de búsqueda debe tener al menos {minLength} caracteres.",
                new { FieldName = fieldName, SearchTerm = searchTerm, MinLength = minLength, CurrentLength = searchTerm.Length }
            );
        }

        if (searchTerm.Length > maxLength)
        {
            throw new ValidationException(
                $"El término de búsqueda no puede exceder {maxLength} caracteres",
                fieldName,
                $"El término de búsqueda es demasiado largo. Máximo {maxLength} caracteres permitidos.",
                new { FieldName = fieldName, SearchTerm = searchTerm, MaxLength = maxLength, CurrentLength = searchTerm.Length }
            );
        }
    }

    /// <summary>
    /// Valida parámetros de paginación
    /// </summary>
    public static void ValidatePagination(int page, int pageSize, int maxPageSize = 100)
    {
        if (page < 1)
        {
            throw new ValidationException(
                "El número de página debe ser mayor a 0",
                "Page",
                "El número de página debe ser un valor positivo.",
                new { Page = page, MinPage = 1 }
            );
        }

        if (pageSize < 1 || pageSize > maxPageSize)
        {
            throw new ValidationException(
                $"El tamaño de página debe estar entre 1 y {maxPageSize}",
                "PageSize",
                $"El tamaño de página debe ser un valor entre 1 y {maxPageSize}.",
                new { PageSize = pageSize, MinPageSize = 1, MaxPageSize = maxPageSize }
            );
        }
    }
}
