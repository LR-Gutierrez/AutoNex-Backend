# Arquitectura del Sistema — AutoNex

## 1. Descripción General

AutoNex es una API REST diseñada para gestionar la operación completa de un taller mecánico automotriz. El sistema abarca desde la administración de clientes y vehículos hasta el control de inventarios, órdenes de servicio, alertas predictivas de mantenimiento basadas en kilometraje, gestión financiera y notificaciones automatizadas vía WhatsApp.

## 2. Stack Tecnológico

| Componente | Tecnología | Versión | Justificación |
|---|---|---|---|
| Runtime | ASP.NET Core | 10.0 | Último LTS, alto rendimiento, ecosistema maduro |
| Lenguaje | C# | 14 (implícito) | Tipado fuerte, nullable habilitado, records para DTOs |
| ORM | Entity Framework Core | 10.x | ORM oficial de Microsoft, migrations integradas |
| Base de Datos | PostgreSQL | 16+ | Robusto, open-source, excelente para relaciones complejas |
| Autenticación | JWT Bearer | — | Stateless, ideal para API REST + frontend futuro |
| API Style | Controller-based | — | Mejor organización para proyectos con múltiples recursos |
| Mapeo | Manual con extension methods | — | Sin dependencias adicionales, control total |
| Validación | FluentValidation | — | Validación declarativa y desacoplada |
| Documentación | Swagger / OpenAPI | — | Generación automática de spec |
| Notificaciones | Twilio API | — | API oficial de WhatsApp Business |

## 3. Arquitectura de Capas (Layered Architecture)

```
┌──────────────────────────────────────────────┐
│              API Layer (Controllers)          │  ← HTTP Request / Response
├──────────────────────────────────────────────┤
│           Application Layer (Services)        │  ← Lógica de negocio
├──────────────────────────────────────────────┤
│         Domain Layer (Models / Entities)      │  ← Entidades del negocio
├──────────────────────────────────────────────┤
│      Infrastructure Layer (Data / Repos)      │  ← EF Core, PostgreSQL, Twilio
└──────────────────────────────────────────────┘
```

### 3.1 Domain Layer — `Models/`

Contiene las entidades del dominio y enumeraciones. Sin dependencias externas.

### 3.2 Application Layer — `Services/` + `DTOs/`

Orquesta la lógica de negocio. Transforma entidades a DTOs y viceversa. Coordina repositorios y servicios externos.

### 3.3 Infrastructure Layer — `Data/`

DbContext, migraciones, configuraciones de entidades (Fluent API), implementaciones concretas de repositorios.

### 3.4 API Layer — `Controllers/`

Endpoints RESTful. Maneja autenticación, autorización, validación de entrada y formateo de respuestas.

## 4. Modelo de Datos

### 4.1 Diagrama de Entidades

```
User (1) ──< (N) ServiceOrder
Client (1) ──< (N) Vehicle
Vehicle (1) ──< (N) ServiceOrder
Vehicle (1) ── (1) MileageAlert
ServiceOrder (1) ──< (N) ServiceOrderItem
ServiceOrderItem (N) >── (1) Consumable (nullable)
Service (1) ──< (N) ServiceOrderItem
Supplier (1) ──< (N) Consumable
User (1) ──< (N) FinancialRecord
```

### 4.2 Tablas y Columnas

| Tabla | Columnas Clave |
|---|---|
| **Users** | Id, FullName, Email, PasswordHash, Role (enum: Admin/Mechanic/Receptionist), Phone, IsActive, CreatedAt, UpdatedAt |
| **Clients** | Id, FullName, Phone, Email, Address, IsDeleted, CreatedAt, UpdatedAt |
| **Vehicles** | Id, ClientId (FK), Brand, Model, Year, LicensePlate, VIN, IsDeleted, CreatedAt, UpdatedAt |
| **Suppliers** | Id, Name, ContactPerson, Phone, Email, IsDeleted, CreatedAt, UpdatedAt |
| **Consumables** | Id, Name, Category (enum: Oil/SparkPlug/Coolant/Grease/BrakeFluid/Other), StockQuantity, MinStock, UnitPrice, SupplierId (FK), IsDeleted, CreatedAt, UpdatedAt |
| **Tools** | Id, Name, Category (enum: Jack/Wrench/Ratchet/Screwdriver/Hammer/Other), Quantity, Status (enum: Available/Damaged/Lost), PurchaseDate, IsDeleted, CreatedAt, UpdatedAt |
| **Services** | Id, Name, Description, DefaultPrice, IsDeleted, CreatedAt, UpdatedAt |
| **ServiceOrders** | Id, VehicleId (FK), ClientId (FK), UserId (FK), CurrentKm, Date, Status (enum: Open/InProgress/Completed/Cancelled), TotalAmount, Notes, IsDeleted, CreatedAt, UpdatedAt |
| **ServiceOrderItems** | Id, ServiceOrderId (FK), ServiceId (FK), ConsumableId (FK nullable), Quantity, UnitPrice, CreatedAt |
| **MileageAlerts** | Id, VehicleId (FK), LastRecordedKm, EstimatedWeeklyKm, NextAlertKm, LastAlertDate, IsActive, CreatedAt, UpdatedAt |
| **FinancialRecords** | Id, Type (enum: Income/Expense), Category (enum: Services/Suppliers/Rent/Payroll/Utilities/Other), Amount, Description, Date, UserId (FK), IsDeleted, CreatedAt, UpdatedAt |
| **Notifications** | Id, ClientId (FK), VehicleId (FK nullable), Type (enum: WhatsApp/SMS/Email), Recipient, Message, SentAt, Status (enum: Pending/Sent/Failed), CreatedAt |
| **InventoryMovements** | Id, ConsumableId (FK nullable), ToolId (FK nullable), MovementType (enum: In/Out), Quantity, Reference, ReferenceId, Notes, CreatedAt |

### 4.3 Convenciones de Diseño

- **Primary Keys**: `Id` (int) con autoincremento
- **Auditoría**: `CreatedAt`, `UpdatedAt` en todas las tablas
- **Soft Delete**: `IsDeleted` flag en lugar de borrado físico
- **Nullable habilitado**: strings nullable donde aplique
- **Precisión decimal**: `decimal(18,2)` para montos
- **Convención de nombres**: SnakeCase en PostgreSQL (configurado en DbContext)

## 5. Endpoints REST (diseño completo)

### 5.1 Autenticación

```
POST   /api/auth/register     → Crear usuario (Admin only)
POST   /api/auth/login        → Obtener JWT
```

### 5.2 Usuarios

```
GET    /api/users              → Listar usuarios (Admin only)
GET    /api/users/{id}         → Obtener usuario por ID
PUT    /api/users/{id}         → Actualizar usuario (Admin only)
```

### 5.3 Clientes

```
GET    /api/clients            → Listar (filtro por nombre/teléfono)
GET    /api/clients/{id}       → Obtener con vehículos incluidos
POST   /api/clients            → Crear
PUT    /api/clients/{id}       → Actualizar
DELETE /api/clients/{id}       → Soft delete
```

### 5.4 Vehículos

```
GET    /api/vehicles                    → Listar (filtro por placa/cliente)
GET    /api/vehicles/{id}               → Obtener con historial de órdenes
POST   /api/vehicles                    → Crear (asociado a cliente)
PUT    /api/vehicles/{id}               → Actualizar
DELETE /api/vehicles/{id}               → Soft delete
```

### 5.5 Proveedores

```
GET    /api/suppliers           → Listar
POST   /api/suppliers           → Crear
PUT    /api/suppliers/{id}      → Actualizar
DELETE /api/suppliers/{id}      → Soft delete
```

### 5.6 Consumibles

```
GET    /api/consumables                  → Listar (con stock actual)
GET    /api/consumables/low-stock        → Alertas de stock mínimo
POST   /api/consumables                  → Crear
PUT    /api/consumables/{id}             → Actualizar stock/precio
DELETE /api/consumables/{id}             → Soft delete
```

### 5.7 Herramientas

```
GET    /api/tools                → Listar (filtro por categoría/estado)
POST   /api/tools                → Crear
PUT    /api/tools/{id}           → Actualizar estado/cantidad
DELETE /api/tools/{id}           → Soft delete
```

### 5.8 Servicios (Catálogo)

```
GET    /api/services             → Listar servicios
POST   /api/services             → Crear servicio
PUT    /api/services/{id}        → Actualizar precio/nombre
DELETE /api/services/{id}        → Soft delete
```

### 5.9 Órdenes de Servicio

```
GET    /api/service-orders                    → Listar (filtro por fecha/cliente/vehículo/estado)
GET    /api/service-orders/{id}               → Obtener con items
POST   /api/service-orders                    → Crear (descuenta stock automáticamente)
PUT    /api/service-orders/{id}               → Actualizar (items, notas)
PATCH  /api/service-orders/{id}/status        → Cambiar estado (Open→InProgress→Completed→Cancelled)
```

### 5.10 Alertas de Kilometraje

```
GET    /api/mileage-alerts                   → Alertas activas
GET    /api/mileage-alerts/due               → Alertas próximas a vencer
POST   /api/mileage-alerts                   → Configurar alerta para vehículo
PUT    /api/mileage-alerts/{id}              → Actualizar km estimados/semana
POST   /api/mileage-alerts/{id}/send         → Enviar recordatorio manual
```

### 5.11 Finanzas

```
GET    /api/financial-records                → Listar (filtro por tipo/fecha/categoría)
POST   /api/financial-records                → Crear registro (ingreso o egreso)
PUT    /api/financial-records/{id}           → Actualizar
DELETE /api/financial-records/{id}           → Soft delete
GET    /api/financial-records/summary        → Resumen por período (ingresos vs egresos)
GET    /api/financial-records/by-category    → Agrupado por categoría
```

### 5.12 Notificaciones

```
GET    /api/notifications                    → Historial de mensajes enviados
POST   /api/notifications/send-whatsapp      → Enviar mensaje manual (disparador)
```

## 6. Políticas de Respuesta API

### Formato estándar de respuesta exitosa:
```json
{
  "data": { ... },
  "success": true,
  "message": "Operación exitosa"
}
```

### Formato de error:
```json
{
  "data": null,
  "success": false,
  "message": "Descripción del error",
  "errors": {
    "fieldName": ["Error de validación 1"]
  }
}
```

### Códigos HTTP:
- `200` OK
- `201` Created
- `204` No Content (DELETE)
- `400` Bad Request (validación)
- `401` Unauthorized
- `403` Forbidden
- `404` Not Found
- `409` Conflict

## 7. Autenticación y Autorización

### Roles:

| Rol | Permisos |
|---|---|
| **Admin** | Acceso total a todos los módulos |
| **Mechanic** | CRUD órdenes de servicio, consulta de clientes/vehículos, consulta de inventarios |
| **Receptionist** | CRUD clientes/vehículos, creación de órdenes, consulta de inventarios (sin modificar) |

### Esquema JWT:

```json
{
  "sub": "userId",
  "email": "user@email.com",
  "role": "Admin",
  "exp": 1234567890
}
```

### Políticas de contraseña:
- Mínimo 8 caracteres
- Al menos una mayúscula, una minúscula y un número
- Hash con BCrypt

## 8. Configuraciones de Infraestructura

### Connection String (PostgreSQL):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AutoNex;Username=postgres;Password=your_password"
  }
}
```

### JWT Settings:
```json
{
  "Jwt": {
    "Key": "supersecretkeywithatleast32characters!",
    "Issuer": "AutoNex",
    "Audience": "AutoNexApi",
    "ExpireMinutes": 60
  }
}
```

### Twilio (para etapa 6):
```json
{
  "Twilio": {
    "AccountSid": "",
    "AuthToken": "",
    "FromNumber": ""
  }
}
```

## 9. Estándares de Código

- **Nomenclatura**: PascalCase para clases/métodos/propiedades, camelCase para parámetros/variables locales
- **Archivos**: Un archivo por clase
- **DTOs**: Sufijo `Request`/`Response` (ej. `CreateClientRequest`, `ClientResponse`)
- **Async**: Métodos asíncronos con sufijo `Async`
- **Validación**: FluentValidation separada de los controllers
- **Mapeo**: Manual con métodos de extensión estáticos
- **Migrations**: `dotnet ef migrations add <Name>`
- **Routing**: Atributos `[Route("api/[controller]")]` en controllers
- **Inyección de dependencias**: Constructor injection en services y controllers

## 10. Configuración Inicial del Proyecto

```bash
# Agregar paquetes NuGet
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package FluentValidation.AspNetCore
dotnet add package BCrypt.Net-Next
dotnet add package Twilio

# Crear migración inicial
dotnet ef migrations add InitialCreate
dotnet ef database update

# Ejecutar proyecto
dotnet run --project AutoNex
```

## 11. Estructura de Directorios Final

```
AutoNex/
├── Models/
│   ├── User.cs
│   ├── Client.cs
│   ├── Vehicle.cs
│   ├── Supplier.cs
│   ├── Consumable.cs
│   ├── Tool.cs
│   ├── Service.cs
│   ├── ServiceOrder.cs
│   ├── ServiceOrderItem.cs
│   ├── MileageAlert.cs
│   ├── FinancialRecord.cs
│   ├── Notification.cs
│   └── InventoryMovement.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   └── AuthResponse.cs
│   ├── Clients/
│   │   ├── CreateClientRequest.cs
│   │   ├── UpdateClientRequest.cs
│   │   └── ClientResponse.cs
│   ├── Vehicles/
│   ├── Suppliers/
│   ├── Consumables/
│   ├── Tools/
│   ├── Services/
│   ├── ServiceOrders/
│   ├── MileageAlerts/
│   ├── FinancialRecords/
│   └── Notifications/
├── Enums/
│   ├── UserRole.cs
│   ├── ConsumableCategory.cs
│   ├── ToolCategory.cs
│   ├── ToolStatus.cs
│   ├── ServiceOrderStatus.cs
│   ├── FinancialRecordType.cs
│   ├── FinancialCategory.cs
│   ├── NotificationType.cs
│   ├── NotificationStatus.cs
│   └── MovementType.cs
├── Data/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── ClientConfiguration.cs
│   │   ├── VehicleConfiguration.cs
│   │   └── ...
│   └── Migrations/
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IClientService.cs
│   │   ├── IVehicleService.cs
│   │   └── ...
│   └── Implementations/
│       ├── AuthService.cs
│       ├── ClientService.cs
│       ├── VehicleService.cs
│       └── ...
├── Controllers/
│   ├── AuthController.cs
│   ├── ClientsController.cs
│   ├── VehiclesController.cs
│   ├── SuppliersController.cs
│   ├── ConsumablesController.cs
│   ├── ToolsController.cs
│   ├── ServicesController.cs
│   ├── ServiceOrdersController.cs
│   ├── MileageAlertsController.cs
│   ├── FinancialRecordsController.cs
│   └── NotificationsController.cs
├── Middleware/
│   ├── ExceptionMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Helpers/
│   ├── MappingExtensions.cs
│   └── PasswordHelper.cs
├── Validators/
│   ├── CreateClientValidator.cs
│   ├── CreateVehicleValidator.cs
│   └── ...
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

## 12. Flujo de Alerta de Kilometraje (Lógica de Negocio)

1. Al crear una **Orden de Servicio**, se registra el `CurrentKm` del vehículo
2. Se solicita al cliente un estimado de **km recorridos por semana**
3. El sistema calcula la **próxima alerta** según el servicio realizado:
   - Cambio de aceite → próximo aviso en `CurrentKm + 5000`
   - Cambio de bujías → próximo aviso en `CurrentKm + 30000`
   - (Los umbrales se definen por tipo de servicio)
4. Un job programado (o consulta manual) revisa diariamente los vehículos cuyo `NextAlertKm` está próximo
5. Cuando `CurrentKm + (EstimatedWeeklyKm * 2) >= NextAlertKm`, se dispara la notificación
6. La notificación se envía vía WhatsApp (Twilio) con el mensaje configurado
7. Queda registrada en la tabla `Notifications` con su estado

## 13. Consideraciones de Seguridad

- **Passwords**: Hash con BCrypt (salt incorporado)
- **JWT**: Tokens con expiración, firmados con clave simétrica de 256 bits mínimo
- **HTTPS**: Redirección forzada en producción
- **SQL Injection**: Prevenido por EF Core (parameterized queries)
- **CORS**: Configurado para el origen del frontend (Angular)
- **Rate Limiting**: Considerar para endpoints de autenticación
- **Validación**: Todos los inputs validados con FluentValidation antes de procesar

## 14. Próximos Pasos — Etapa 1 (Fundación)

- [ ] Agregar paquetes NuGet necesarios (Npgsql, JWT, FluentValidation, BCrypt)
- [ ] Crear carpetas del proyecto según estructura definida
- [ ] Implementar `Enums/` (todos los enumeradores)
- [ ] Implementar `Models/` (User, Client, Vehicle, Supplier)
- [ ] Implementar `Data/AppDbContext.cs` con configuraciones Fluent API
- [ ] Crear migración inicial y aplicar a PostgreSQL
- [ ] Implementar `DTOs/` para Auth, Clients, Vehicles, Suppliers
- [ ] Implementar `Services/` (AuthService, ClientService, VehicleService, SupplierService)
- [ ] Implementar `Controllers/` (AuthController, ClientsController, VehiclesController, SuppliersController)
- [ ] Implementar `Middleware/ExceptionMiddleware.cs`
- [ ] Configurar `Program.cs` (DI, JWT, Swagger, CORS)
- [ ] Seed de usuario Admin por defecto
- [ ] Probar endpoints con Swagger
