using Common.Models;

namespace Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica un usuario y genera un token JWT
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Refresca un token JWT usando un refresh token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    
    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    
    /// <summary>
    /// Valida un token JWT
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Obtiene la información del usuario desde un token
    /// </summary>
    Task<UserInfo?> GetUserFromTokenAsync(string token);
    
    /// <summary>
    /// Revoca un refresh token
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
}
