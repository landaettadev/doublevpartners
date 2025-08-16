-- =============================================
-- Script de Tipos de Tabla Personalizados
-- Base de Datos: LabDev
-- =============================================

USE [LabDev]
GO

-- Crear tipo de tabla para detalles de factura
CREATE TYPE [dbo].[InvoiceDetailTVP] AS TABLE
(
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL
)
GO

-- Crear tipo de tabla para búsquedas
CREATE TYPE [dbo].[SearchCriteriaTVP] AS TABLE
(
    [SearchType] NVARCHAR(20) NOT NULL,
    [SearchValue] NVARCHAR(100) NOT NULL
)
GO
