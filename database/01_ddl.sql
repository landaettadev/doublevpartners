-- =============================================
-- Script DDL - Creación de Tablas
-- Base de Datos: LabDev
-- =============================================

USE [LabDev]
GO

-- Crear tabla de Clientes
CREATE TABLE [dbo].[Clients] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NULL,
    [Phone] NVARCHAR(20) NULL,
    [Address] NVARCHAR(200) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    [UpdatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

-- Crear tabla de Productos
CREATE TABLE [dbo].[Products] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [IsActive] BIT DEFAULT 1 NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    [UpdatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

-- Crear tabla de Facturas (Encabezado)
CREATE TABLE [dbo].[Invoices] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [InvoiceNumber] NVARCHAR(20) NOT NULL,
    [ClientId] INT NOT NULL,
    [InvoiceDate] DATETIME2 DEFAULT GETDATE() NOT NULL,
    [Subtotal] DECIMAL(18,2) NOT NULL,
    [TaxAmount] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(20) DEFAULT 'Active' NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    [UpdatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Invoices_Clients] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Clients] ([Id]),
    CONSTRAINT [UQ_Invoices_InvoiceNumber] UNIQUE NONCLUSTERED ([InvoiceNumber] ASC)
)
GO

-- Crear tabla de Detalles de Factura
CREATE TABLE [dbo].[InvoiceDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [InvoiceId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT [PK_InvoiceDetails] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_InvoiceDetails_Invoices] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id]),
    CONSTRAINT [FK_InvoiceDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
)
GO

-- Crear índices para mejorar performance
CREATE NONCLUSTERED INDEX [IX_Clients_Name] ON [dbo].[Clients] ([Name] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Products_Name] ON [dbo].[Products] ([Name] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Invoices_ClientId] ON [dbo].[Invoices] ([ClientId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Invoices_InvoiceDate] ON [dbo].[Invoices] ([InvoiceDate] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_InvoiceDetails_InvoiceId] ON [dbo].[InvoiceDetails] ([InvoiceId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_InvoiceDetails_ProductId] ON [dbo].[InvoiceDetails] ([ProductId] ASC)
GO

-- Crear constraint para validar precios positivos
ALTER TABLE [dbo].[Products] ADD CONSTRAINT [CK_Products_Price] CHECK ([Price] > 0)
GO

ALTER TABLE [dbo].[InvoiceDetails] ADD CONSTRAINT [CK_InvoiceDetails_Quantity] CHECK ([Quantity] > 0)
GO

ALTER TABLE [dbo].[InvoiceDetails] ADD CONSTRAINT [CK_InvoiceDetails_UnitPrice] CHECK ([UnitPrice] > 0)
GO

ALTER TABLE [dbo].[InvoiceDetails] ADD CONSTRAINT [CK_InvoiceDetails_Total] CHECK ([Total] > 0)
GO

-- Crear constraint para validar totales de factura
ALTER TABLE [dbo].[Invoices] ADD CONSTRAINT [CK_Invoices_Subtotal] CHECK ([Subtotal] >= 0)
GO

ALTER TABLE [dbo].[Invoices] ADD CONSTRAINT [CK_Invoices_TaxAmount] CHECK ([TaxAmount] >= 0)
GO

ALTER TABLE [dbo].[Invoices] ADD CONSTRAINT [CK_Invoices_Total] CHECK ([Total] > 0)
GO
