namespace Common.Models;

/// <summary>
/// Configuración para JWT
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// Claims personalizados para JWT
/// </summary>
public static class JwtClaims
{
    public const string UserId = "userId";
    public const string Username = "username";
    public const string Email = "email";
    public const string Roles = "roles";
    public const string FirstName = "firstName";
    public const string LastName = "lastName";
}
