using Application.Interfaces;
using Common.Errors;
using Common.Interfaces;
using Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

/// <summary>
/// Servicio de autenticación
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger _logger;

    public AuthService(
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings,
        ILogger logger)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException(
                    "Credenciales incompletas",
                    "Credentials",
                    "El nombre de usuario y contraseña son requeridos.",
                    new { Username = request.Username, HasPassword = !string.IsNullOrEmpty(request.Password) }
                );
            }

            // Buscar usuario
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                throw new UnauthorizedException(
                    "Credenciales inválidas",
                    "INVALID_CREDENTIALS",
                    "El nombre de usuario o contraseña son incorrectos.",
                    new { Username = request.Username }
                );
            }

            // Verificar si el usuario está activo
            if (!user.IsActive)
            {
                throw new UnauthorizedException(
                    "Usuario inactivo",
                    "USER_INACTIVE",
                    "Su cuenta ha sido desactivada. Contacte al administrador.",
                    new { UserId = user.Id, Username = user.Username }
                );
            }

            // Verificar contraseña
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException(
                    "Credenciales inválidas",
                    "INVALID_CREDENTIALS",
                    "El nombre de usuario o contraseña son incorrectos.",
                    new { Username = request.Username }
                );
            }

            // Obtener roles del usuario
            var roles = await _userRepository.GetUserRolesAsync(user.Id);

            // Generar tokens
            var token = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            // Actualizar último login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Usuario {Username} autenticado exitosamente", user.Username);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                },
                Roles = roles,
                Message = "Autenticación exitosa"
            };
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la autenticación del usuario {Username}", request.Username);
            throw new UnauthorizedException(
                "Error durante la autenticación",
                "AUTH_ERROR",
                "Ocurrió un error durante la autenticación. Por favor, intente nuevamente.",
                new { Username = request.Username, Error = ex.Message }
            );
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException(
                    "Datos de registro incompletos",
                    "RegistrationData",
                    "Todos los campos obligatorios deben ser completados.",
                    new { HasUsername = !string.IsNullOrEmpty(request.Username), HasPassword = !string.IsNullOrEmpty(request.Password) }
                );
            }

            // Verificar si el usuario ya existe
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new ConflictException(
                    "Usuario ya existe",
                    "USER_ALREADY_EXISTS",
                    "Ya existe un usuario con ese nombre de usuario.",
                    new { Username = request.Username }
                );
            }

            // Verificar si el email ya existe
            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                throw new ConflictException(
                    "Email ya existe",
                    "EMAIL_ALREADY_EXISTS",
                    "Ya existe un usuario con ese email.",
                    new { Email = request.Email }
                );
            }

            // Crear nuevo usuario
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Asignar rol por defecto (Usuario)
            await _userRepository.AssignRoleAsync(createdUser.Id, 2); // Rol "User"

            _logger.LogInformation("Nuevo usuario registrado: {Username}", createdUser.Username);

            return new AuthResponse
            {
                Success = true,
                Token = string.Empty, // No generar token automáticamente
                RefreshToken = string.Empty,
                ExpiresAt = DateTime.UtcNow,
                User = new UserInfo
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    CreatedAt = createdUser.CreatedAt,
                    LastLoginAt = createdUser.LastLoginAt
                },
                Roles = new List<string> { "User" },
                Message = "Usuario registrado exitosamente. Por favor, inicie sesión."
            };
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro del usuario {Username}", request.Username);
            throw new UnauthorizedException(
                "Error durante el registro",
                "REGISTRATION_ERROR",
                "Ocurrió un error durante el registro. Por favor, intente nuevamente.",
                new { Username = request.Username, Error = ex.Message }
            );
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Por ahora, implementación básica
            // En una implementación real, validaríamos el refresh token
            throw new NotImplementedException("Refresh token no implementado aún");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el refresh del token");
            throw new UnauthorizedException(
                "Error durante el refresh del token",
                "REFRESH_TOKEN_ERROR",
                "No se pudo refrescar el token. Por favor, inicie sesión nuevamente.",
                new { Error = ex.Message }
            );
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(
                    "Usuario no encontrado",
                    "User",
                    userId,
                    "No se encontró el usuario especificado.",
                    new { UserId = userId }
                );
            }

            // Verificar contraseña actual
            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new ValidationException(
                    "Contraseña actual incorrecta",
                    "CurrentPassword",
                    "La contraseña actual no es correcta.",
                    new { UserId = userId }
                );
            }

            // Cambiar contraseña
            user.PasswordHash = HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Contraseña cambiada para el usuario {UserId}", userId);
            return true;
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña para el usuario {UserId}", userId);
            throw new UnauthorizedException(
                "Error al cambiar contraseña",
                "CHANGE_PASSWORD_ERROR",
                "No se pudo cambiar la contraseña. Por favor, intente nuevamente.",
                new { UserId = userId, Error = ex.Message }
            );
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserFromTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtClaims.UserId);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Por ahora, implementación básica
            // En una implementación real, invalidaríamos el refresh token
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar refresh token");
            return false;
        }
    }

    #region Private Methods

    private string GenerateJwtToken(User user, List<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(JwtClaims.UserId, user.Id.ToString()),
            new(JwtClaims.Username, user.Username),
            new(JwtClaims.Email, user.Email),
            new(JwtClaims.FirstName, user.FirstName),
            new(JwtClaims.LastName, user.LastName),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        // Agregar roles como claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim(JwtClaims.Roles, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }

    #endregion
}
