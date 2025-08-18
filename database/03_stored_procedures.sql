-- =============================================
-- Script de Stored Procedures
-- Base de Datos: LabDev
-- =============================================

USE [LabDev]
GO

-- =============================================
-- PROCEDIMIENTOS PARA CLIENTES
-- =============================================

-- Obtener todos los clientes
CREATE PROCEDURE [dbo].[sp_GetClients]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Name],
        [Email],
        [Phone],
        [Address],
        [CreatedAt],
        [UpdatedAt]
    FROM [dbo].[Clients]
    ORDER BY [Name]
END
GO

-- Obtener cliente por ID
CREATE PROCEDURE [dbo].[sp_GetClientById]
    @ClientId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Name],
        [Email],
        [Phone],
        [Address],
        [CreatedAt],
        [UpdatedAt]
    FROM [dbo].[Clients]
    WHERE [Id] = @ClientId
END
GO

-- =============================================
-- PROCEDIMIENTOS PARA PRODUCTOS
-- =============================================

-- Obtener todos los productos activos
CREATE PROCEDURE [dbo].[sp_GetProducts]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Name],
        [Description],
        [Price],
        [ImageUrl],
        [IsActive],
        [CreatedAt],
        [UpdatedAt]
    FROM [dbo].[Products]
    WHERE [IsActive] = 1
    ORDER BY [Name]
END
GO

-- Obtener producto por ID
CREATE PROCEDURE [dbo].[sp_GetProductById]
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Name],
        [Description],
        [Price],
        [ImageUrl],
        [IsActive],
        [CreatedAt],
        [UpdatedAt]
    FROM [dbo].[Products]
    WHERE [Id] = @ProductId AND [IsActive] = 1
END
GO

-- =============================================
-- PROCEDIMIENTOS PARA FACTURAS
-- =============================================

-- Crear nueva factura con detalles
CREATE PROCEDURE [dbo].[sp_CreateInvoice]
    @InvoiceNumber NVARCHAR(20),
    @ClientId INT,
    @InvoiceDate DATETIME2,
    @Subtotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @Total DECIMAL(18,2),
    @InvoiceDetails [dbo].[InvoiceDetailTVP] READONLY,
    @InvoiceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insertar encabezado de factura
        INSERT INTO [dbo].[Invoices] (
            [InvoiceNumber],
            [ClientId],
            [InvoiceDate],
            [Subtotal],
            [TaxAmount],
            [Total],
            [Status],
            [CreatedAt],
            [UpdatedAt]
        )
        VALUES (
            @InvoiceNumber,
            @ClientId,
            @InvoiceDate,
            @Subtotal,
            @TaxAmount,
            @Total,
            'Active',
            GETDATE(),
            GETDATE()
        );
        
        SET @InvoiceId = SCOPE_IDENTITY();
        
        -- Insertar detalles de factura
        INSERT INTO [dbo].[InvoiceDetails] (
            [InvoiceId],
            [ProductId],
            [Quantity],
            [UnitPrice],
            [Total],
            [CreatedAt]
        )
        SELECT 
            @InvoiceId,
            [ProductId],
            [Quantity],
            [UnitPrice],
            [Total],
            GETDATE()
        FROM @InvoiceDetails;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        THROW;
    END CATCH
END
GO

-- Obtener factura por ID con detalles
CREATE PROCEDURE [dbo].[sp_GetInvoiceById]
    @InvoiceId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Obtener encabezado
    SELECT 
        i.[Id],
        i.[InvoiceNumber],
        i.[ClientId],
        c.[Name] AS ClientName,
        i.[InvoiceDate],
        i.[Subtotal],
        i.[TaxAmount],
        i.[Total],
        i.[Status],
        i.[CreatedAt],
        i.[UpdatedAt]
    FROM [dbo].[Invoices] i
    INNER JOIN [dbo].[Clients] c ON i.[ClientId] = c.[Id]
    WHERE i.[Id] = @InvoiceId;
    
    -- Obtener detalles
    SELECT 
        id.[Id],
        id.[ProductId],
        p.[Name] AS ProductName,
        p.[ImageUrl],
        id.[Quantity],
        id.[UnitPrice],
        id.[Total]
    FROM [dbo].[InvoiceDetails] id
    INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
    WHERE id.[InvoiceId] = @InvoiceId;
END
GO

-- Buscar facturas por cliente o número
CREATE PROCEDURE [dbo].[sp_SearchInvoices]
    @SearchType NVARCHAR(20),
    @SearchValue NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @SearchType = 'Client'
    BEGIN
        SELECT 
            i.[Id],
            i.[InvoiceNumber],
            i.[ClientId],
            c.[Name] AS ClientName,
            i.[InvoiceDate],
            i.[Subtotal],
            i.[TaxAmount],
            i.[Total],
            i.[Status],
            i.[CreatedAt]
        FROM [dbo].[Invoices] i
        INNER JOIN [dbo].[Clients] c ON i.[ClientId] = c.[Id]
        WHERE c.[Name] LIKE '%' + @SearchValue + '%'
        ORDER BY i.[InvoiceDate] DESC;
    END
    ELSE IF @SearchType = 'InvoiceNumber'
    BEGIN
        SELECT 
            i.[Id],
            i.[InvoiceNumber],
            i.[ClientId],
            c.[Name] AS ClientName,
            i.[InvoiceDate],
            i.[Subtotal],
            i.[TaxAmount],
            i.[Total],
            i.[Status],
            i.[CreatedAt]
        FROM [dbo].[Invoices] i
        INNER JOIN [dbo].[Clients] c ON i.[ClientId] = c.[Id]
        WHERE i.[InvoiceNumber] LIKE '%' + @SearchValue + '%'
        ORDER BY i.[InvoiceDate] DESC;
    END
END
GO

-- Obtener todas las facturas (para listado)
CREATE PROCEDURE [dbo].[sp_GetInvoices]
    @PageSize INT = 20,
    @PageNumber INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        i.[Id],
        i.[InvoiceNumber],
        i.[ClientId],
        c.[Name] AS ClientName,
        i.[InvoiceDate],
        i.[Subtotal],
        i.[TaxAmount],
        i.[Total],
        i.[Status],
        i.[CreatedAt]
    FROM [dbo].[Invoices] i
    INNER JOIN [dbo].[Clients] c ON i.[ClientId] = c.[Id]
    ORDER BY i.[InvoiceDate] DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Obtener total de registros
    SELECT COUNT(*) AS TotalRecords
    FROM [dbo].[Invoices];
END
GO

-- Obtener detalles de factura por ID de factura
CREATE PROCEDURE [dbo].[sp_GetInvoiceDetailsByInvoiceId]
    @InvoiceId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        id.[ProductId],
        id.[Quantity],
        id.[UnitPrice],
        id.[Total]
    FROM [dbo].[InvoiceDetails] id
    WHERE id.[InvoiceId] = @InvoiceId
    ORDER BY id.[Id];
END
GO

-- Verificar si número de factura existe con información adicional
CREATE OR ALTER PROCEDURE [dbo].[sp_CheckInvoiceNumberExists]
    @InvoiceNumber NVARCHAR(20),
    @Exists BIT OUTPUT,
    @InvoiceId INT = NULL OUTPUT,
    @ClientName NVARCHAR(100) = NULL OUTPUT,
    @InvoiceDate DATETIME2 = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @InvoiceExists TABLE (
        Id INT,
        ClientName NVARCHAR(100),
        InvoiceDate DATETIME2
    );
    
    INSERT INTO @InvoiceExists (Id, ClientName, InvoiceDate)
    SELECT 
        i.[Id],
        c.[Name],
        i.[InvoiceDate]
    FROM [dbo].[Invoices] i
    INNER JOIN [dbo].[Clients] c ON i.[ClientId] = c.[Id]
    WHERE i.[InvoiceNumber] = @InvoiceNumber;
    
    IF EXISTS (SELECT 1 FROM @InvoiceExists)
    BEGIN
        SET @Exists = 1;
        SELECT 
            @InvoiceId = Id,
            @ClientName = ClientName,
            @InvoiceDate = InvoiceDate
        FROM @InvoiceExists;
    END
    ELSE
    BEGIN
        SET @Exists = 0;
        SET @InvoiceId = NULL;
        SET @ClientName = NULL;
        SET @InvoiceDate = NULL;
    END
END
GO

-- =============================================
-- Stored Procedures para Productos
-- =============================================

-- Crear producto
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateProduct]
    @Name NVARCHAR(100),
    @Description NVARCHAR(500),
    @Price DECIMAL(18,2),
    @ImageUrl NVARCHAR(255),
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (@Name, @Description, @Price, @ImageUrl, @IsActive, GETDATE(), GETDATE());
    
    SET @ProductId = SCOPE_IDENTITY();
END
GO

-- Actualizar producto
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateProduct]
    @ProductId INT,
    @Name NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @Price DECIMAL(18,2) = NULL,
    @ImageUrl NVARCHAR(255) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Products]
    SET 
        [Name] = ISNULL(@Name, [Name]),
        [Description] = ISNULL(@Description, [Description]),
        [Price] = ISNULL(@Price, [Price]),
        [ImageUrl] = ISNULL(@ImageUrl, [ImageUrl]),
        [IsActive] = ISNULL(@IsActive, [IsActive]),
        [UpdatedAt] = GETDATE()
    WHERE [Id] = @ProductId;
END
GO

-- Eliminar producto (soft delete)
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteProduct]
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Products]
    SET [IsActive] = 0, [UpdatedAt] = GETDATE()
    WHERE [Id] = @ProductId;
END
GO

-- Cambiar estado del producto
CREATE OR ALTER PROCEDURE [dbo].[sp_ToggleProductStatus]
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Products]
    SET [IsActive] = ~[IsActive], [UpdatedAt] = GETDATE()
    WHERE [Id] = @ProductId;
END
GO

-- Obtener productos activos
CREATE OR ALTER PROCEDURE [dbo].[sp_GetActiveProducts]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Name],
        [Description],
        [Price],
        [ImageUrl],
        [IsActive],
        [CreatedAt],
        [UpdatedAt]
    FROM [dbo].[Products]
    WHERE [IsActive] = 1
    ORDER BY [Name];
END
GO

-- Buscar productos
CREATE OR ALTER PROCEDURE [dbo].[sp_SearchProducts]
    @SearchTerm NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Name],
        [Description],
        [Price],
        [ImageUrl],
        [IsActive],
        [CreatedAt],
        [UpdatedAt]
    FROM [dbo].[Products]
    WHERE [IsActive] = 1
        AND ([Name] LIKE '%' + @SearchTerm + '%' 
             OR [Description] LIKE '%' + @SearchTerm + '%')
    ORDER BY [Name];
END
GO
