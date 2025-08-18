using Application.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controlador para autenticación y gestión de usuarios
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Autentica un usuario y genera un token JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (Common.Errors.ValidationException ex)
        {
            return BadRequest(new
            {
                error = "Error de validación",
                message = ex.Message,
                userMessage = ex.UserMessage,
                additionalData = ex.AdditionalData,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Common.Errors.UnauthorizedException ex)
        {
            return Unauthorized(new
            {
                error = "Error de autenticación",
                message = ex.Message,
                code = ex.ErrorCode,
                userMessage = ex.UserMessage,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login del usuario {Username}", request.Username);
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error durante la autenticación. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (Common.Errors.ValidationException ex)
        {
            return BadRequest(new
            {
                error = "Error de validación",
                message = ex.Message,
                userMessage = ex.UserMessage,
                additionalData = ex.AdditionalData,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Common.Errors.ConflictException ex)
        {
            return Conflict(new
            {
                error = "Conflicto de datos",
                message = ex.Message,
                code = ex.ErrorCode,
                userMessage = ex.UserMessage,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro del usuario {Username}", request.Username);
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error durante el registro. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Refresca un token JWT usando un refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (Common.Errors.UnauthorizedException ex)
        {
            return Unauthorized(new
            {
                error = "Error de refresh token",
                message = ex.Message,
                code = ex.ErrorCode,
                userMessage = ex.UserMessage,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el refresh del token");
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error durante el refresh del token. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Cambia la contraseña de un usuario autenticado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            // Obtener el ID del usuario desde el token
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new
                {
                    error = "Token inválido",
                    message = "No se pudo identificar al usuario.",
                    timestamp = DateTime.UtcNow
                });
            }

            var success = await _authService.ChangePasswordAsync(userId, request);
            if (success)
            {
                return Ok(new
                {
                    message = "Contraseña cambiada exitosamente",
                    timestamp = DateTime.UtcNow
                });
            }

            return BadRequest(new
            {
                error = "Error al cambiar contraseña",
                message = "No se pudo cambiar la contraseña.",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Common.Errors.ValidationException ex)
        {
            return BadRequest(new
            {
                error = "Error de validación",
                message = ex.Message,
                userMessage = ex.UserMessage,
                additionalData = ex.AdditionalData,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Common.Errors.NotFoundException ex)
        {
            return NotFound(new
            {
                error = "Usuario no encontrado",
                message = ex.Message,
                code = ex.ErrorCode,
                userMessage = ex.UserMessage,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña");
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error al cambiar la contraseña. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtiene información del usuario autenticado
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetProfile()
    {
        try
        {
            // Obtener el ID del usuario desde el token
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new
                {
                    error = "Token inválido",
                    message = "No se pudo identificar al usuario.",
                    timestamp = DateTime.UtcNow
                });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    error = "Usuario no encontrado",
                    message = "No se encontró el usuario especificado.",
                    timestamp = DateTime.UtcNow
                });
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del usuario");
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error al obtener el perfil. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Valida un token JWT
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new
                {
                    error = "Token requerido",
                    message = "El token es requerido para la validación.",
                    timestamp = DateTime.UtcNow
                });
            }

            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(new
            {
                isValid = isValid,
                message = isValid ? "Token válido" : "Token inválido",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar token");
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error al validar el token. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Revoca un refresh token
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var success = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
            if (success)
            {
                return Ok(new
                {
                    message = "Token revocado exitosamente",
                    timestamp = DateTime.UtcNow
                });
            }

            return BadRequest(new
            {
                error = "Error al revocar token",
                message = "No se pudo revocar el token.",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar token");
            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ocurrió un error al revocar el token. Por favor, intente nuevamente.",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
