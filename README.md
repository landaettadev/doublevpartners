# Sistema de Facturación - Prueba Técnica Double V Partners

## Descripción
Sistema completo de facturación desarrollado con .NET Core + Angular para la prueba técnica de desarrollador Full Stack.

## Stack Tecnológico
- **Backend**: ASP.NET Core 8.0, Clean Architecture
- **Frontend**: Angular 17, TypeScript
- **Base de Datos**: SQL Server
- **ORM**: ADO.NET (sin Entity Framework)

## Estructura del Proyecto
```
├─ /database          # Scripts de base de datos
├─ /backend          # API .NET Core
├─ /frontend         # Aplicación Angular
└─ /tests            # Pruebas unitarias e integración
```

## Requisitos Previos
- .NET 8.0 SDK
- Node.js 18+
- SQL Server 2019+
- Visual Studio Code

## Instalación y Configuración

### Base de Datos
1. Ejecutar scripts en orden:
   - `01_ddl.sql` - Crear tablas
   - `02_types.sql` - Crear tipos
   - `03_stored_procedures.sql` - Crear procedimientos
   - `99_seed.sql` - Datos de ejemplo

### Backend
```bash
cd backend/src/Api
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend
npm install
ng serve
```

## Funcionalidades
- ✅ Creación de facturas
- ✅ Búsqueda de facturas
- ✅ Gestión de clientes y productos
- ✅ Cálculos automáticos (subtotal, impuestos, total)

## Arquitectura
- **Clean Architecture** con separación de capas
- **SOLID Principles** implementados
- **Stored Procedures** para todas las operaciones de BD
- **DTOs** para transferencia de datos

## Testing
- Pruebas unitarias del backend
- Pruebas de integración con base de datos
- Testing del frontend con Karma/Jasmine

## Autor
[Tu Nombre] - Prueba Técnica Double V Partners

## Fecha
Enero 2024
