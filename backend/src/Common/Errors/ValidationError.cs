namespace Common.Errors;

/// <summary>
/// Representa un error de validación específico
/// </summary>
public class ValidationError
{
    public string FieldName { get; set; }
    public string ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public object? AttemptedValue { get; set; }
    public string? Suggestion { get; set; }

    public ValidationError(string fieldName, string errorMessage, string? errorCode = null, object? attemptedValue = null, string? suggestion = null)
    {
        FieldName = fieldName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        AttemptedValue = attemptedValue;
        Suggestion = suggestion;
    }
}

/// <summary>
/// Resultado de una operación con información de éxito/error
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }
    public IReadOnlyList<ValidationError>? ValidationErrors { get; }
    public object? AdditionalData { get; }

    private Result(bool isSuccess, T? value = default, string? errorMessage = null, string? errorCode = null, IReadOnlyList<ValidationError>? validationErrors = null, object? additionalData = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
        AdditionalData = additionalData;
    }

    public static Result<T> Success(T value, object? additionalData = null)
    {
        return new Result<T>(true, value, additionalData: additionalData);
    }

    public static Result<T> Failure(string errorMessage, string? errorCode = null, object? additionalData = null)
    {
        return new Result<T>(false, errorMessage: errorMessage, errorCode: errorCode, additionalData: additionalData);
    }

    public static Result<T> ValidationFailure(IReadOnlyList<ValidationError> validationErrors, object? additionalData = null)
    {
        return new Result<T>(false, errorMessage: "Error de validación", errorCode: "VALIDATION_ERROR", validationErrors: validationErrors, additionalData: additionalData);
    }

    public static Result<T> ValidationFailure(string fieldName, string errorMessage, object? additionalData = null)
    {
        var validationError = new ValidationError(fieldName, errorMessage);
        return ValidationFailure(new List<ValidationError> { validationError }, additionalData);
    }
}

/// <summary>
/// Resultado de una operación sin valor de retorno
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }
    public IReadOnlyList<ValidationError>? ValidationErrors { get; }
    public object? AdditionalData { get; }

    private Result(bool isSuccess, string? errorMessage = null, string? errorCode = null, IReadOnlyList<ValidationError>? validationErrors = null, object? additionalData = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
        AdditionalData = additionalData;
    }

    public static Result Success(object? additionalData = null)
    {
        return new Result(true, additionalData: additionalData);
    }

    public static Result Failure(string errorMessage, string? errorCode = null, object? additionalData = null)
    {
        return new Result(false, errorMessage: errorMessage, errorCode: errorCode, additionalData: additionalData);
    }

    public static Result ValidationFailure(IReadOnlyList<ValidationError> validationErrors, object? additionalData = null)
    {
        return new Result(false, errorMessage: "Error de validación", errorCode: "VALIDATION_ERROR", validationErrors: validationErrors, additionalData: additionalData);
    }

    public static Result ValidationFailure(string fieldName, string errorMessage, object? additionalData = null)
    {
        var validationError = new ValidationError(fieldName, errorMessage);
        return ValidationFailure(new List<ValidationError> { validationError }, additionalData);
    }
}
