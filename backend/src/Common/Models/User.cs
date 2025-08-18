using System.ComponentModel.DataAnnotations;

namespace Common.Models;

/// <summary>
/// Entidad de usuario para autenticación
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public List<UserRole> UserRoles { get; set; } = new();
}

/// <summary>
/// Entidad de rol de usuario
/// </summary>
public class UserRole
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int RoleId { get; set; }
    
    public User User { get; set; } = null!;
    
    public Role Role { get; set; } = null!;
}

/// <summary>
/// Entidad de rol
/// </summary>
public class Role
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public List<UserRole> UserRoles { get; set; } = new();
}
