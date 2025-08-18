using Common.Models;

namespace Application.Interfaces;

/// <summary>
/// Interfaz para el repositorio de usuarios
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    Task<User?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    Task<User> CreateAsync(User user);
    
    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    Task<User> UpdateAsync(User user);
    
    /// <summary>
    /// Elimina un usuario
    /// </summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene todos los usuarios activos
    /// </summary>
    Task<IEnumerable<User>> GetActiveUsersAsync();
    
    /// <summary>
    /// Obtiene los roles de un usuario
    /// </summary>
    Task<List<string>> GetUserRolesAsync(int userId);
    
    /// <summary>
    /// Asigna un rol a un usuario
    /// </summary>
    Task<bool> AssignRoleAsync(int userId, int roleId);
    
    /// <summary>
    /// Remueve un rol de un usuario
    /// </summary>
    Task<bool> RemoveRoleAsync(int userId, int roleId);
    
    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    Task<bool> UserHasRoleAsync(int userId, string roleName);
}
