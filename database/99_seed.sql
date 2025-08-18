-- =============================================
-- Script de Datos de Ejemplo (Seed Data) - IDEMPOTENTE
-- Base de Datos: LabDev
-- =============================================

USE [LabDev]
GO

-- Insertar clientes de ejemplo (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Clients] WHERE [Email] = 'contacto@abc.com')
BEGIN
    INSERT INTO [dbo].[Clients] ([Name], [Email], [Phone], [Address], [CreatedAt], [UpdatedAt])
    VALUES ('Empresa ABC S.A.', 'contacto@abc.com', '+57 300 123 4567', 'Calle 123 #45-67, Bogotá', GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Clients] WHERE [Email] = 'ventas@xyz.com')
BEGIN
    INSERT INTO [dbo].[Clients] ([Name], [Email], [Phone], [Address], [CreatedAt], [UpdatedAt])
    VALUES ('Comercial XYZ Ltda.', 'ventas@xyz.com', '+57 310 987 6543', 'Carrera 78 #90-12, Medellín', GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Clients] WHERE [Email] = 'info@delta.com')
BEGIN
    INSERT INTO [dbo].[Clients] ([Name], [Email], [Phone], [Address], [CreatedAt], [UpdatedAt])
    VALUES ('Servicios Técnicos Delta', 'info@delta.com', '+57 315 555 1234', 'Avenida 5 #23-45, Cali', GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Clients] WHERE [Email] = 'pedidos@omega.com')
BEGIN
    INSERT INTO [dbo].[Clients] ([Name], [Email], [Phone], [Address], [CreatedAt], [UpdatedAt])
    VALUES ('Distribuidora Omega', 'pedidos@omega.com', '+57 320 777 8888', 'Calle 67 #89-01, Barranquilla', GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Clients] WHERE [Email] = 'asesoria@sigma.com')
BEGIN
    INSERT INTO [dbo].[Clients] ([Name], [Email], [Phone], [Address], [CreatedAt], [UpdatedAt])
    VALUES ('Consultoría Sigma', 'asesoria@sigma.com', '+57 318 999 0000', 'Carrera 34 #56-78, Bucaramanga', GETDATE(), GETDATE())
END
GO

-- Insertar productos de ejemplo (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Laptop HP Pavilion')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Laptop HP Pavilion', 'Laptop HP Pavilion 15" Intel Core i5, 8GB RAM, 256GB SSD', 2500000.00, '/images/laptop-hp.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Mouse Inalámbrico Logitech')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Mouse Inalámbrico Logitech', 'Mouse inalámbrico Logitech M185, 1000 DPI, hasta 12 meses de batería', 45000.00, '/images/mouse-logitech.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Teclado Mecánico Corsair')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Teclado Mecánico Corsair', 'Teclado mecánico Corsair K70 RGB MK.2, switches Cherry MX Red', 350000.00, '/images/teclado-corsair.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Monitor Samsung 24"')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Monitor Samsung 24"', 'Monitor Samsung 24" Full HD, 1920x1080, 60Hz, Panel VA', 450000.00, '/images/monitor-samsung.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Webcam Logitech C920')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Webcam Logitech C920', 'Webcam Logitech C920 Pro HD, 1080p, autofocus, micrófono integrado', 180000.00, '/images/webcam-logitech.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Auriculares Sony WH-1000XM4')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Auriculares Sony WH-1000XM4', 'Auriculares inalámbricos Sony WH-1000XM4, cancelación de ruido', 1200000.00, '/images/auriculares-sony.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Disco Duro Externo WD')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Disco Duro Externo WD', 'Disco duro externo Western Digital 1TB, USB 3.0, portátil', 180000.00, '/images/disco-wd.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Memoria RAM Kingston')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Memoria RAM Kingston', 'Memoria RAM Kingston DDR4 8GB 2666MHz, CL19, para desktop', 120000.00, '/images/ram-kingston.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Procesador Intel Core i7')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Procesador Intel Core i7', 'Procesador Intel Core i7-10700K, 8 núcleos, 3.8GHz base', 1800000.00, '/images/procesador-intel.jpg', 1, GETDATE(), GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [Name] = 'Tarjeta Gráfica NVIDIA RTX')
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('Tarjeta Gráfica NVIDIA RTX', 'Tarjeta gráfica NVIDIA RTX 3060 12GB GDDR6, Ray Tracing', 2800000.00, '/images/gpu-nvidia.jpg', 1, GETDATE(), GETDATE())
END
GO

-- Insertar facturas de ejemplo (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Invoices] WHERE [InvoiceNumber] = 'FAC-001-2024')
BEGIN
    INSERT INTO [dbo].[Invoices] ([InvoiceNumber], [ClientId], [InvoiceDate], [Subtotal], [TaxAmount], [Total], [Status], [CreatedAt], [UpdatedAt])
    SELECT 'FAC-001-2024', c.[Id], DATEADD(day, -5, GETDATE()), 2950000.00, 560500.00, 3510500.00, 'Active', GETDATE(), GETDATE()
    FROM [dbo].[Clients] c WHERE c.[Email] = 'contacto@abc.com'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Invoices] WHERE [InvoiceNumber] = 'FAC-002-2024')
BEGIN
    INSERT INTO [dbo].[Invoices] ([InvoiceNumber], [ClientId], [InvoiceDate], [Subtotal], [TaxAmount], [Total], [Status], [CreatedAt], [UpdatedAt])
    SELECT 'FAC-002-2024', c.[Id], DATEADD(day, -3, GETDATE()), 630000.00, 119700.00, 749700.00, 'Active', GETDATE(), GETDATE()
    FROM [dbo].[Clients] c WHERE c.[Email] = 'ventas@xyz.com'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Invoices] WHERE [InvoiceNumber] = 'FAC-003-2024')
BEGIN
    INSERT INTO [dbo].[Invoices] ([InvoiceNumber], [ClientId], [InvoiceDate], [Subtotal], [TaxAmount], [Total], [Status], [CreatedAt], [UpdatedAt])
    SELECT 'FAC-003-2024', c.[Id], DATEADD(day, -1, GETDATE()), 1980000.00, 376200.00, 2356200.00, 'Active', GETDATE(), GETDATE()
    FROM [dbo].[Clients] c WHERE c.[Email] = 'info@delta.com'
END
GO

-- Insertar detalles de factura para FAC-001-2024 (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceDetails] id 
               INNER JOIN [dbo].[Invoices] i ON id.[InvoiceId] = i.[Id] 
               INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
               WHERE i.[InvoiceNumber] = 'FAC-001-2024' AND p.[Name] = 'Laptop HP Pavilion')
BEGIN
    INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
    SELECT i.[Id], p.[Id], 1, 2500000.00, 2500000.00, GETDATE()
    FROM [dbo].[Invoices] i, [dbo].[Products] p
    WHERE i.[InvoiceNumber] = 'FAC-001-2024' AND p.[Name] = 'Laptop HP Pavilion'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceDetails] id 
               INNER JOIN [dbo].[Invoices] i ON id.[InvoiceId] = i.[Id] 
               INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
               WHERE i.[InvoiceNumber] = 'FAC-001-2024' AND p.[Name] = 'Mouse Inalámbrico Logitech')
BEGIN
    INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
    SELECT i.[Id], p.[Id], 1, 45000.00, 45000.00, GETDATE()
    FROM [dbo].[Invoices] i, [dbo].[Products] p
    WHERE i.[InvoiceNumber] = 'FAC-001-2024' AND p.[Name] = 'Mouse Inalámbrico Logitech'
END
GO

-- Insertar detalles de factura para FAC-002-2024 (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceDetails] id 
               INNER JOIN [dbo].[Invoices] i ON id.[InvoiceId] = i.[Id] 
               INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
               WHERE i.[InvoiceNumber] = 'FAC-002-2024' AND p.[Name] = 'Monitor Samsung 24"')
BEGIN
    INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
    SELECT i.[Id], p.[Id], 1, 450000.00, 450000.00, GETDATE()
    FROM [dbo].[Invoices] i, [dbo].[Products] p
    WHERE i.[InvoiceNumber] = 'FAC-002-2024' AND p.[Name] = 'Monitor Samsung 24"'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceDetails] id 
               INNER JOIN [dbo].[Invoices] i ON id.[InvoiceId] = i.[Id] 
               INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
               WHERE i.[InvoiceNumber] = 'FAC-002-2024' AND p.[Name] = 'Webcam Logitech C920')
BEGIN
    INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
    SELECT i.[Id], p.[Id], 1, 180000.00, 180000.00, GETDATE()
    FROM [dbo].[Invoices] i, [dbo].[Products] p
    WHERE i.[InvoiceNumber] = 'FAC-002-2024' AND p.[Name] = 'Webcam Logitech C920'
END
GO

-- Insertar detalles de factura para FAC-003-2024 (solo si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceDetails] id 
               INNER JOIN [dbo].[Invoices] i ON id.[InvoiceId] = i.[Id] 
               INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
               WHERE i.[InvoiceNumber] = 'FAC-003-2024' AND p.[Name] = 'Procesador Intel Core i7')
BEGIN
    INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
    SELECT i.[Id], p.[Id], 1, 1800000.00, 1800000.00, GETDATE()
    FROM [dbo].[Invoices] i, [dbo].[Products] p
    WHERE i.[InvoiceNumber] = 'FAC-003-2024' AND p.[Name] = 'Procesador Intel Core i7'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceDetails] id 
               INNER JOIN [dbo].[Invoices] i ON id.[InvoiceId] = i.[Id] 
               INNER JOIN [dbo].[Products] p ON id.[ProductId] = p.[Id]
               WHERE i.[InvoiceNumber] = 'FAC-003-2024' AND p.[Name] = 'Memoria RAM Kingston')
BEGIN
    INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
    SELECT i.[Id], p.[Id], 2, 120000.00, 240000.00, GETDATE()
    FROM [dbo].[Invoices] i, [dbo].[Products] p
    WHERE i.[InvoiceNumber] = 'FAC-003-2024' AND p.[Name] = 'Memoria RAM Kingston'
END
GO

-- Verificar datos insertados
SELECT 'Clientes insertados:' AS Info, COUNT(*) AS Cantidad FROM [dbo].[Clients]
UNION ALL
SELECT 'Productos insertados:', COUNT(*) FROM [dbo].[Products]
UNION ALL
SELECT 'Facturas insertadas:', COUNT(*) FROM [dbo].[Invoices]
UNION ALL
SELECT 'Detalles insertados:', COUNT(*) FROM [dbo].[InvoiceDetails]
GO
