# Sistema de Facturación - Prueba Técnica Double V Partners

## 📋 Descripción del Proyecto

Este proyecto implementa un sistema completo de facturación que incluye:

- **Backend**: API REST en .NET Core 8 con arquitectura limpia
- **Frontend**: Aplicación Angular 17 con componentes standalone
- **Base de Datos**: SQL Server con stored procedures y tipos de tabla personalizados

## 🏗️ Arquitectura del Proyecto

### Backend (.NET Core)
```
backend/
├── src/
│   ├── Api/                 # Capa de presentación (Controllers, Middleware)
│   ├── Application/         # Capa de aplicación (Services, DTOs, Interfaces)
│   ├── Infrastructure/      # Capa de infraestructura (Repositories, Database)
│   └── Common/             # Capa común (Errores, Interfaces compartidas)
```

### Frontend (Angular)
```
frontend/
├── src/
│   ├── app/
│   │   ├── core/           # Servicios HTTP, interceptors
│   │   ├── features/       # Componentes de funcionalidad
│   │   └── shared/         # Modelos y componentes compartidos
│   └── environments/       # Configuración de entornos
```

## 🚀 Funcionalidades Implementadas

### 1. Creación de Facturas
- ✅ Formulario completo con validaciones
- ✅ Selección de cliente desde catálogo
- ✅ Agregar/remover productos dinámicamente
- ✅ Cálculo automático de totales e IVA (19%)
- ✅ Validación de número de factura único
- ✅ Carga automática de precios e imágenes de productos

### 2. Búsqueda de Facturas
- ✅ Búsqueda por cliente o número de factura
- ✅ Deshabilitación inteligente de campos según tipo de búsqueda
- ✅ Resultados en tabla con paginación
- ✅ Visualización de detalles de factura

### 3. Catálogos
- ✅ Gestión de clientes
- ✅ Gestión de productos con imágenes
- ✅ APIs REST para consulta de datos

## 🛠️ Tecnologías Utilizadas

### Backend
- **.NET Core 8** - Framework principal
- **ADO.NET** - Acceso a base de datos (sin Entity Framework)
- **SQL Server** - Base de datos
- **Stored Procedures** - Lógica de negocio en base de datos
- **Arquitectura Limpia** - Separación de responsabilidades

### Frontend
- **Angular 17** - Framework de frontend
- **Bootstrap 5** - Framework CSS
- **Reactive Forms** - Formularios reactivos
- **RxJS** - Programación reactiva
- **Standalone Components** - Componentes independientes

### Base de Datos
- **SQL Server** - Motor de base de datos
- **Stored Procedures** - Operaciones CRUD
- **Table-Valued Parameters** - Parámetros complejos
- **Transacciones** - Integridad de datos
- **Índices** - Optimización de consultas

## 📦 Requisitos Previos

- **.NET 8 SDK**
- **Node.js 18+**
- **SQL Server 2019+**
- **SQL Server Management Studio** (opcional)

## 🔧 Instalación y Configuración

### 1. Clonar el Repositorio
```bash
git clone <url-del-repositorio>
cd Doublevpartners
```

### 2. Configurar Base de Datos
1. Abrir SQL Server Management Studio
2. Conectar al servidor: `DESKTOP-D4KSLA3`
3. Crear base de datos: `LabDev`
4. Ejecutar scripts en orden:
   ```sql
   -- 1. Crear tablas
   EXEC database/01_ddl.sql
   
   -- 2. Crear tipos personalizados
   EXEC database/02_types.sql
   
   -- 3. Crear stored procedures
   EXEC database/03_stored_procedures.sql
   
   -- 4. Insertar datos de ejemplo
   EXEC database/99_seed.sql
   ```

### 3. Configurar Backend
1. Navegar al directorio del backend:
   ```bash
   cd backend/src/Api
   ```

2. Verificar configuración en `appsettings.json`:
   ```json
   {
     "Database": {
       "ConnectionString": "Server=DESKTOP-D4KSLA3;Database=LabDev;User Id=developer;Password=abc123ABC;Encrypt=False;"
     }
   }
   ```

3. Restaurar dependencias:
   ```bash
   dotnet restore
   ```

4. Ejecutar el proyecto:
   ```bash
   dotnet run
   ```

El backend estará disponible en: `https://localhost:7001`

### 4. Configurar Frontend
1. Navegar al directorio del frontend:
   ```bash
   cd frontend
   ```

2. Instalar dependencias:
   ```bash
   npm install
   ```

3. Verificar configuración en `src/environments/environment.ts`:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7001'
   };
   ```

4. Ejecutar el proyecto:
   ```bash
   npm start
   ```

El frontend estará disponible en: `http://localhost:4200`

## 📱 Uso de la Aplicación

### Crear Factura
1. Navegar a "Crear Factura"
2. Ingresar número de factura (debe ser único)
3. Seleccionar cliente del catálogo
4. Agregar productos con cantidades
5. Los totales se calculan automáticamente
6. Guardar la factura

### Buscar Facturas
1. Navegar a "Buscar Facturas"
2. Seleccionar tipo de búsqueda (Cliente o Número)
3. Ingresar criterio de búsqueda
4. Ver resultados en tabla
5. Hacer clic en el ojo para ver detalles

## 🧪 Pruebas

### Backend
```bash
cd backend
dotnet test
```

### Frontend
```bash
cd frontend
npm test
```

## 📊 Estructura de Base de Datos

### Tablas Principales
- **Clients**: Información de clientes
- **Products**: Catálogo de productos
- **Invoices**: Encabezado de facturas
- **InvoiceDetails**: Detalles de facturas

### Stored Procedures
- `sp_CreateInvoice`: Crear factura con detalles
- `sp_GetInvoiceById`: Obtener factura por ID
- `sp_SearchInvoices`: Buscar facturas por criterios
- `sp_GetClients`: Obtener catálogo de clientes
- `sp_GetProducts`: Obtener catálogo de productos

## 🔒 Características de Seguridad

- **Validación de entrada** en frontend y backend
- **Transacciones** para operaciones complejas
- **Manejo de errores** centralizado
- **CORS** configurado para desarrollo
- **Validación de número de factura único**

## 📈 Mejoras Futuras

- [ ] Autenticación y autorización
- [ ] Logs de auditoría
- [ ] Reportes en PDF
- [ ] Envío de facturas por email
- [ ] Dashboard con métricas
- [ ] API de notificaciones
- [ ] Tests de integración
- [ ] Dockerización
- [ ] CI/CD pipeline

## 🐛 Solución de Problemas

### Error de Conexión a Base de Datos
- Verificar que SQL Server esté ejecutándose
- Confirmar credenciales en `appsettings.json`
- Verificar que la base de datos `LabDev` exista

### Error de CORS
- Verificar que el backend esté ejecutándose en `https://localhost:7001`
- Confirmar configuración CORS en `Program.cs`

### Error de Compilación Angular
- Verificar versión de Node.js (18+)
- Limpiar cache: `npm cache clean --force`
- Eliminar `node_modules` y reinstalar

## 📝 Notas de Desarrollo

- El proyecto sigue los principios SOLID
- No se utiliza Entity Framework (requerimiento de la prueba)
- Todas las operaciones de BD usan stored procedures
- El frontend usa componentes standalone de Angular 17
- La arquitectura backend sigue el patrón Repository

## 👨‍💻 Autor

Desarrollado para la Prueba Técnica de Double V Partners

## 📄 Licencia

Este proyecto es para fines de evaluación técnica.
