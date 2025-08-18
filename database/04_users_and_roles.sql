-- =============================================
-- Script para crear tablas de usuarios y roles
-- =============================================

USE LabDev;
GO

-- Crear tabla de roles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(200),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Crear tabla de usuarios
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(100) NOT NULL,
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2
    );
END
GO

-- Crear tabla de roles de usuario
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE UserRoles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (RoleId) REFERENCES Roles(Id),
        UNIQUE(UserId, RoleId)
    );
END
GO

-- Insertar roles por defecto
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Name, Description) VALUES ('Admin', 'Administrador del sistema');
END

IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'User')
BEGIN
    INSERT INTO Roles (Name, Description) VALUES ('User', 'Usuario estándar');
END

-- Insertar usuario administrador por defecto
-- Contraseña: Admin123! (hash SHA256)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsActive)
    VALUES ('admin', 'admin@doublevpartners.com', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Admin', 'System', 1);
    
    -- Asignar rol de administrador
    DECLARE @AdminUserId INT = SCOPE_IDENTITY();
    DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');
    
    INSERT INTO UserRoles (UserId, RoleId) VALUES (@AdminUserId, @AdminRoleId);
END

-- Insertar usuario de prueba
-- Contraseña: User123! (hash SHA256)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'user')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsActive)
    VALUES ('user', 'user@doublevpartners.com', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Test', 'User', 1);
    
    -- Asignar rol de usuario
    DECLARE @TestUserId INT = SCOPE_IDENTITY();
    DECLARE @UserRoleId INT = (SELECT Id FROM Roles WHERE Name = 'User');
    
    INSERT INTO UserRoles (UserId, RoleId) VALUES (@TestUserId, @UserRoleId);
END
GO

-- Crear índices para mejorar el rendimiento
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);
GO

PRINT 'Tablas de usuarios y roles creadas exitosamente.';
PRINT 'Usuario admin: admin / Admin123!';
PRINT 'Usuario test: user / User123!';
GO
