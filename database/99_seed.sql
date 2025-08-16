-- =============================================
-- Script de Datos de Ejemplo (Seed Data)
-- Base de Datos: LabDev
-- =============================================

USE [LabDev]
GO

-- Insertar clientes de ejemplo
INSERT INTO [dbo].[Clients] ([Name], [Email], [Phone], [Address], [CreatedAt], [UpdatedAt])
VALUES 
    ('Empresa ABC S.A.', 'contacto@abc.com', '+57 300 123 4567', 'Calle 123 #45-67, Bogotá', GETDATE(), GETDATE()),
    ('Comercial XYZ Ltda.', 'ventas@xyz.com', '+57 310 987 6543', 'Carrera 78 #90-12, Medellín', GETDATE(), GETDATE()),
    ('Servicios Técnicos Delta', 'info@delta.com', '+57 315 555 1234', 'Avenida 5 #23-45, Cali', GETDATE(), GETDATE()),
    ('Distribuidora Omega', 'pedidos@omega.com', '+57 320 777 8888', 'Calle 67 #89-01, Barranquilla', GETDATE(), GETDATE()),
    ('Consultoría Sigma', 'asesoria@sigma.com', '+57 318 999 0000', 'Carrera 34 #56-78, Bucaramanga', GETDATE(), GETDATE())
GO

-- Insertar productos de ejemplo
INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [IsActive], [CreatedAt], [UpdatedAt])
VALUES 
    ('Laptop HP Pavilion', 'Laptop HP Pavilion 15" Intel Core i5, 8GB RAM, 256GB SSD', 2500000.00, '/images/laptop-hp.jpg', 1, GETDATE(), GETDATE()),
    ('Mouse Inalámbrico Logitech', 'Mouse inalámbrico Logitech M185, 1000 DPI, hasta 12 meses de batería', 45000.00, '/images/mouse-logitech.jpg', 1, GETDATE(), GETDATE()),
    ('Teclado Mecánico Corsair', 'Teclado mecánico Corsair K70 RGB MK.2, switches Cherry MX Red', 350000.00, '/images/teclado-corsair.jpg', 1, GETDATE(), GETDATE()),
    ('Monitor Samsung 24"', 'Monitor Samsung 24" Full HD, 1920x1080, 60Hz, Panel VA', 450000.00, '/images/monitor-samsung.jpg', 1, GETDATE(), GETDATE()),
    ('Webcam Logitech C920', 'Webcam Logitech C920 Pro HD, 1080p, autofocus, micrófono integrado', 180000.00, '/images/webcam-logitech.jpg', 1, GETDATE(), GETDATE()),
    ('Auriculares Sony WH-1000XM4', 'Auriculares inalámbricos Sony WH-1000XM4, cancelación de ruido', 1200000.00, '/images/auriculares-sony.jpg', 1, GETDATE(), GETDATE()),
    ('Disco Duro Externo WD', 'Disco duro externo Western Digital 1TB, USB 3.0, portátil', 180000.00, '/images/disco-wd.jpg', 1, GETDATE(), GETDATE()),
    ('Memoria RAM Kingston', 'Memoria RAM Kingston DDR4 8GB 2666MHz, CL19, para desktop', 120000.00, '/images/ram-kingston.jpg', 1, GETDATE(), GETDATE()),
    ('Procesador Intel Core i7', 'Procesador Intel Core i7-10700K, 8 núcleos, 3.8GHz base', 1800000.00, '/images/procesador-intel.jpg', 1, GETDATE(), GETDATE()),
    ('Tarjeta Gráfica NVIDIA RTX', 'Tarjeta gráfica NVIDIA RTX 3060 12GB GDDR6, Ray Tracing', 2800000.00, '/images/gpu-nvidia.jpg', 1, GETDATE(), GETDATE())
GO

-- Insertar facturas de ejemplo
INSERT INTO [dbo].[Invoices] ([InvoiceNumber], [ClientId], [InvoiceDate], [Subtotal], [TaxAmount], [Total], [Status], [CreatedAt], [UpdatedAt])
VALUES 
    ('FAC-001-2024', 1, DATEADD(day, -5, GETDATE()), 2950000.00, 560500.00, 3510500.00, 'Active', GETDATE(), GETDATE()),
    ('FAC-002-2024', 2, DATEADD(day, -3, GETDATE()), 630000.00, 119700.00, 749700.00, 'Active', GETDATE(), GETDATE()),
    ('FAC-003-2024', 3, DATEADD(day, -1, GETDATE()), 1980000.00, 376200.00, 2356200.00, 'Active', GETDATE(), GETDATE())
GO

-- Insertar detalles de factura para FAC-001-2024
INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
VALUES 
    (1, 1, 1, 2500000.00, 2500000.00, GETDATE()),  -- Laptop HP
    (1, 2, 1, 45000.00, 45000.00, GETDATE())        -- Mouse Logitech
GO

-- Insertar detalles de factura para FAC-002-2024
INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
VALUES 
    (2, 4, 1, 450000.00, 450000.00, GETDATE()),    -- Monitor Samsung
    (2, 5, 1, 180000.00, 180000.00, GETDATE())      -- Webcam Logitech
GO

-- Insertar detalles de factura para FAC-003-2024
INSERT INTO [dbo].[InvoiceDetails] ([InvoiceId], [ProductId], [Quantity], [UnitPrice], [Total], [CreatedAt])
VALUES 
    (3, 9, 1, 1800000.00, 1800000.00, GETDATE()),  -- Procesador Intel
    (3, 8, 2, 120000.00, 240000.00, GETDATE())      -- 2x RAM Kingston
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
