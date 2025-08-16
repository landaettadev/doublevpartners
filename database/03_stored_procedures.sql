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

-- Verificar si número de factura existe
CREATE PROCEDURE [dbo].[sp_CheckInvoiceNumberExists]
    @InvoiceNumber NVARCHAR(20),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[Invoices] WHERE [InvoiceNumber] = @InvoiceNumber)
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
END
GO
