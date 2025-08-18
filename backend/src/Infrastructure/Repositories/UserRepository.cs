using Application.Interfaces;
using Common.Models;
using Infrastructure.Db;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

/// <summary>
/// Repositorio de usuarios
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetUserById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al obtener usuario desde la base de datos",
                "GetUserById",
                ex.Message,
                new { UserId = id, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al obtener usuario",
                "GetUserById",
                ex.Message,
                new { UserId = id, Error = ex.Message }
            );
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetUserByUsername", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al obtener usuario por nombre de usuario",
                "GetUserByUsername",
                ex.Message,
                new { Username = username, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al obtener usuario por nombre de usuario",
                "GetUserByUsername",
                ex.Message,
                new { Username = username, Error = ex.Message }
            );
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetUserByEmail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al obtener usuario por email",
                "GetUserByEmail",
                ex.Message,
                new { Email = email, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al obtener usuario por email",
                "GetUserByEmail",
                ex.Message,
                new { Email = email, Error = ex.Message }
            );
        }
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_CreateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

            var userId = await command.ExecuteScalarAsync();
            user.Id = Convert.ToInt32(userId);

            return user;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al crear usuario en la base de datos",
                "CreateUser",
                ex.Message,
                new { Username = user.Username, Email = user.Email, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al crear usuario",
                "CreateUser",
                ex.Message,
                new { Username = user.Username, Email = user.Email, Error = ex.Message }
            );
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_UpdateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", user.Id);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@LastLoginAt", user.LastLoginAt);

            await command.ExecuteNonQueryAsync();
            return user;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al actualizar usuario en la base de datos",
                "UpdateUser",
                ex.Message,
                new { UserId = user.Id, Username = user.Username, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al actualizar usuario",
                "UpdateUser",
                ex.Message,
                new { UserId = user.Id, Username = user.Username, Error = ex.Message }
            );
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_DeleteUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al eliminar usuario de la base de datos",
                "DeleteUser",
                ex.Message,
                new { UserId = id, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al eliminar usuario",
                "DeleteUser",
                ex.Message,
                new { UserId = id, Error = ex.Message }
            );
        }
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetActiveUsers", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<User>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(MapUserFromReader(reader));
            }

            return users;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al obtener usuarios activos desde la base de datos",
                "GetActiveUsers",
                ex.Message,
                new { SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al obtener usuarios activos",
                "GetActiveUsers",
                ex.Message,
                new { Error = ex.Message }
            );
        }
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetUserRoles", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);

            var roles = new List<string>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                roles.Add(reader.GetString(0)); // RoleName
            }

            return roles;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al obtener roles del usuario desde la base de datos",
                "GetUserRoles",
                ex.Message,
                new { UserId = userId, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al obtener roles del usuario",
                "GetUserRoles",
                ex.Message,
                new { UserId = userId, Error = ex.Message }
            );
        }
    }

    public async Task<bool> AssignRoleAsync(int userId, int roleId)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_AssignUserRole", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al asignar rol al usuario en la base de datos",
                "AssignUserRole",
                ex.Message,
                new { UserId = userId, RoleId = roleId, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al asignar rol al usuario",
                "AssignUserRole",
                ex.Message,
                new { UserId = userId, RoleId = roleId, Error = ex.Message }
            );
        }
    }

    public async Task<bool> RemoveRoleAsync(int userId, int roleId)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_RemoveUserRole", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al remover rol del usuario en la base de datos",
                "RemoveUserRole",
                ex.Message,
                new { UserId = userId, RoleId = roleId, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error inesperado al remover rol del usuario",
                "RemoveUserRole",
                ex.Message,
                new { UserId = userId, RoleId = roleId, Error = ex.Message }
            );
        }
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName)
    {
        try
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new Common.Errors.DatabaseException(
                "Error al verificar rol del usuario",
                "UserHasRole",
                ex.Message,
                new { UserId = userId, RoleName = roleName, Error = ex.Message }
            );
        }
    }

    #region Private Methods

    private static User MapUserFromReader(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            FirstName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            LastName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            IsActive = reader.GetBoolean(6),
            CreatedAt = reader.GetDateTime(7),
            LastLoginAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
    }

    #endregion
}
