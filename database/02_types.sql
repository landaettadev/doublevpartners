-- =============================================
-- Script de Tipos - Table-Valued Parameters (TVP)
-- Base de Datos: LabDev
-- =============================================

USE [LabDev]
GO

-- Crear tipo para detalles de factura
CREATE TYPE [dbo].[InvoiceDetailTVP] AS TABLE
(
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL
)
GO

-- Crear tipo para búsqueda de facturas
CREATE TYPE [dbo].[InvoiceSearchTVP] AS TABLE
(
    [SearchType] NVARCHAR(20) NOT NULL,  -- 'Client' o 'InvoiceNumber'
    [SearchValue] NVARCHAR(100) NOT NULL
)
GO

-- Crear tipo para filtros de catálogo
CREATE TYPE [dbo].[CatalogFilterTVP] AS TABLE
(
    [FilterType] NVARCHAR(20) NOT NULL,  -- 'Active', 'Name', 'PriceRange'
    [FilterValue] NVARCHAR(100) NOT NULL
)
GO
