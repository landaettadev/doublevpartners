-- =============================================
-- Stored Procedures para gestión de usuarios
-- =============================================

USE LabDev;
GO

-- =============================================
-- Obtener usuario por ID
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetUserById' AND xtype='P')
    DROP PROCEDURE sp_GetUserById
GO

CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.FirstName,
        u.LastName,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt
    FROM Users u
    WHERE u.Id = @UserId;
END
GO

-- =============================================
-- Obtener usuario por nombre de usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetUserByUsername' AND xtype='P')
    DROP PROCEDURE sp_GetUserByUsername
GO

CREATE PROCEDURE sp_GetUserByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.FirstName,
        u.LastName,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt
    FROM Users u
    WHERE u.Username = @Username;
END
GO

-- =============================================
-- Obtener usuario por email
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetUserByEmail' AND xtype='P')
    DROP PROCEDURE sp_GetUserByEmail
GO

CREATE PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.FirstName,
        u.LastName,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt
    FROM Users u
    WHERE u.Email = @Email;
END
GO

-- =============================================
-- Crear nuevo usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_CreateUser' AND xtype='P')
    DROP PROCEDURE sp_CreateUser
GO

CREATE PROCEDURE sp_CreateUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @IsActive BIT,
    @CreatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsActive, CreatedAt)
        VALUES (@Username, @Email, @PasswordHash, @FirstName, @LastName, @IsActive, @CreatedAt);
        
        SELECT SCOPE_IDENTITY() AS UserId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        THROW;
    END CATCH
END
GO

-- =============================================
-- Actualizar usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_UpdateUser' AND xtype='P')
    DROP PROCEDURE sp_UpdateUser
GO

CREATE PROCEDURE sp_UpdateUser
    @UserId INT,
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @IsActive BIT,
    @LastLoginAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        UPDATE Users 
        SET 
            Username = @Username,
            Email = @Email,
            PasswordHash = @PasswordHash,
            FirstName = @FirstName,
            LastName = @LastName,
            IsActive = @IsActive,
            LastLoginAt = @LastLoginAt
        WHERE Id = @UserId;
        
        IF @@ROWCOUNT = 0
            THROW 50001, 'Usuario no encontrado', 1;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        THROW;
    END CATCH
END
GO

-- =============================================
-- Eliminar usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_DeleteUser' AND xtype='P')
    DROP PROCEDURE sp_DeleteUser
GO

CREATE PROCEDURE sp_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Eliminar roles del usuario
        DELETE FROM UserRoles WHERE UserId = @UserId;
        
        -- Eliminar usuario
        DELETE FROM Users WHERE Id = @UserId;
        
        IF @@ROWCOUNT = 0
            THROW 50001, 'Usuario no encontrado', 1;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        THROW;
    END CATCH
END
GO

-- =============================================
-- Obtener usuarios activos
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetActiveUsers' AND xtype='P')
    DROP PROCEDURE sp_GetActiveUsers
GO

CREATE PROCEDURE sp_GetActiveUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.FirstName,
        u.LastName,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt
    FROM Users u
    WHERE u.IsActive = 1
    ORDER BY u.Username;
END
GO

-- =============================================
-- Obtener roles de un usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetUserRoles' AND xtype='P')
    DROP PROCEDURE sp_GetUserRoles
GO

CREATE PROCEDURE sp_GetUserRoles
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT r.Name
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @UserId AND r.IsActive = 1
    ORDER BY r.Name;
END
GO

-- =============================================
-- Asignar rol a usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_AssignUserRole' AND xtype='P')
    DROP PROCEDURE sp_AssignUserRole
GO

CREATE PROCEDURE sp_AssignUserRole
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar que el usuario y rol existen
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @UserId)
            THROW 50001, 'Usuario no encontrado', 1;
            
        IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = @RoleId)
            THROW 50002, 'Rol no encontrado', 1;
        
        -- Asignar rol (ignorar si ya existe)
        IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId)
            VALUES (@UserId, @RoleId);
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        THROW;
    END CATCH
END
GO

-- =============================================
-- Remover rol de usuario
-- =============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_RemoveUserRole' AND xtype='P')
    DROP PROCEDURE sp_RemoveUserRole
GO

CREATE PROCEDURE sp_RemoveUserRole
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DELETE FROM UserRoles 
        WHERE UserId = @UserId AND RoleId = @RoleId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        THROW;
    END CATCH
END
GO

PRINT 'Stored procedures de usuarios creados exitosamente.';
GO
