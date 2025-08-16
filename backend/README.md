# Backend API - Doublev Partners

## Problemas Corregidos

### 1. Errores de Compilación en Repositorios
- **Problema**: Los métodos `GetInt32()`, `GetString()`, etc. del `SqlDataReader` estaban recibiendo nombres de columnas (strings) en lugar de índices numéricos (int).
- **Solución**: Cambié todos los métodos de lectura de datos para usar índices numéricos (0, 1, 2, etc.) en lugar de nombres de columnas.

### 2. Incompatibilidad de Tipos en InvoicesController
- **Problema**: El `InvoiceCreateRequest` no tenía las propiedades `Subtotal`, `TaxAmount` y `Total` que requería el `InvoiceCreateDto`.
- **Solución**: Agregué un método de mapeo que calcula automáticamente estos valores basándose en los detalles de la factura.

### 3. Configuración de Puertos
- **Problema**: La aplicación no tenía configuración explícita de puertos.
- **Solución**: Agregué configuración de Kestrel en `appsettings.json` y `launchSettings.json`.

## Estructura del Proyecto

```
backend/
├── src/
│   ├── Api/                    # API Web principal
│   ├── Application/            # Lógica de negocio y servicios
│   ├── Infrastructure/         # Acceso a datos y repositorios
│   └── Common/                 # Entidades y errores compartidos
├── database/                   # Scripts de base de datos
└── tests/                      # Pruebas unitarias
```

## Tecnologías Utilizadas

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core** (preparado para futuras implementaciones)
- **SQL Server** con stored procedures
- **Swagger/OpenAPI** para documentación
- **CORS** configurado para Angular

## Configuración de Base de Datos

La aplicación está configurada para conectarse a SQL Server con los siguientes parámetros:
- **Servidor**: DESKTOP-D4KSLA3
- **Base de datos**: LabDev
- **Usuario**: developer
- **Contraseña**: abc123ABC

## Cómo Ejecutar

### Opción 1: Usando dotnet run
```bash
cd backend/src/Api
dotnet run
```

### Opción 2: Usando el script de PowerShell
```powershell
cd backend
.\run-api.ps1
```

### Opción 3: Especificando puertos
```bash
cd backend/src/Api
dotnet run --urls "http://localhost:5000;https://localhost:5001"
```

## Endpoints Disponibles

### Test Controller
- `GET /api/test` - Verificar que la API funciona
- `GET /api/test/catalog/clientes` - Obtener clientes de prueba
- `POST /api/test/invoices` - Crear factura de prueba
- `GET /api/test/invoices/by-number/{numero}` - Obtener factura por número

### Invoices Controller
- `POST /api/invoices` - Crear factura
- `GET /api/invoices/{id}` - Obtener factura por ID
- `GET /api/invoices` - Obtener facturas paginadas
- `POST /api/invoices/search` - Buscar facturas
- `GET /api/invoices/check-number/{invoiceNumber}` - Verificar si existe número de factura
- `GET /api/invoices/by-number/{invoiceNumber}` - Obtener factura por número

### Catalog Controller
- `GET /api/catalog/clients` - Obtener clientes
- `GET /api/catalog/clients/{id}` - Obtener cliente por ID
- `GET /api/catalog/products` - Obtener productos
- `GET /api/catalog/products/{id}` - Obtener producto por ID

## Swagger UI

Una vez que la aplicación esté ejecutándose, puedes acceder a la documentación interactiva de la API en:
- **HTTP**: http://localhost:5000/swagger
- **HTTPS**: https://localhost:5001/swagger

## Notas Importantes

1. **Base de Datos**: Asegúrate de que SQL Server esté ejecutándose y que la base de datos `LabDev` exista.
2. **Stored Procedures**: La aplicación espera que existan los stored procedures definidos en `database/03_stored_procedures.sql`.
3. **CORS**: Configurado para permitir conexiones desde `http://localhost:4200` (Angular).
4. **Logging**: Configurado para mostrar información detallada en desarrollo.

## Próximos Pasos

1. Implementar autenticación y autorización
2. Agregar validaciones más robustas
3. Implementar caché para mejorar el rendimiento
4. Agregar pruebas de integración
5. Configurar CI/CD
