# Auditoría: Documentación vs Código

Fecha: 2026-06-11

---

## CRÍTICOS

### 1. Endpoints de Usuarios documentados pero NO implementados
- **ARCHITECTURE.md**:107-113 documenta `GET /api/users`, `GET /api/users/{id}`, `PUT /api/users/{id}`
- **Realidad**: No existe `UsersController.cs`, ni `IUserService`, ni `UserService`
- **API.md** correctamente no los documenta
- **Fix**: Crear el controlador y servicio, o eliminar de ARCHITECTURE.md

### 2. Tabla InventoryMovements documentada pero NO implementada
- **ARCHITECTURE.md**:87 describe la tabla con columnas completas
- **Realidad**: No existe `InventoryMovement.cs`, ni DbSet, ni controlador, ni servicio
- `MovementType.cs` existe como enum pero no se usa
- **Fix**: Implementar el módulo o eliminar de ARCHITECTURE.md

---

## ALTOS

### 3. Register devuelve 200 en vez de 201 Created
- **AuthController.cs**:27 retorna `Ok()` en `POST /api/auth/register`
- Todos los demás POST usan `CreatedAtAction` (201)
- **Fix**: Cambiar a `CreatedAtAction`

### 4. DELETE devuelve 200 en vez de 204 No Content
- **ARCHITECTURE.md**:241 documenta `204 No Content (DELETE)`
- **Realidad**: Todos los `Delete` actions retornan `Ok(ApiResponse<...>)` (200)
- Archivos: ClientsController, VehiclesController, SuppliersController, ConsumablesController, ToolsController, ServicesController, MileageAlertsController, FinancialRecordsController
- **Fix**: Cambiar a `return NoContent()` con `ApiResponse`

### 5. Vehicle `/{id}` no incluye historial de órdenes
- **ARCHITECTURE.md**:129 documenta `GET /api/vehicles/{id} → Obtener con historial de órdenes`
- **Realidad**: `VehicleResponse` solo tiene datos básicos (Brand, Model, Year, LicensePlate, etc.), sin ServiceOrders
- **Fix**: Agregar orders al response o corregir documentación

### 6. RequestLoggingMiddleware documentado pero no existe
- **ARCHITECTURE.md**:419 lista `Middleware/RequestLoggingMiddleware.cs`
- **Realidad**: Solo existe `ExceptionMiddleware.cs` en el directorio Middleware/
- **Fix**: Implementar o eliminar de ARCHITECTURE.md

### 7. Falta autorización por roles en Suppliers, Consumables, Tools
- **ARCHITECTURE.md**:252-256 documenta: Receptionist solo lectura en inventarios
- **Realidad**: `SuppliersController`, `ConsumablesController`, `ToolsController` solo tienen `[Authorize]` (cualquier rol autenticado puede CRUD)
- **Fix**: Agregar `[Authorize(Roles = "Admin")]` en endpoints de escritura

---

## MEDIOS

### 8. Route `api/financial-records` usa kebab en docs, PascalCase en código
- **API.md**:393 y docs usan `api/financial-records`
- **FinancialRecordsController.cs**:11 usa `[Route("api/[controller]")]` → resuelve a `api/FinancialRecords`
- **Nota**: ASP.NET Core routing es case-insensitive por defecto, por lo que funciona igual. Pero es inconsistente con ServiceOrdersController que sí usa kebab explícito (`api/service-orders`)
- **Fix**: Agregar `[Route("api/financial-records")]` explícito como los demás

### 9. MileageAlerts `/due` como ruta separada en ARCHITECTURE.md
- **ARCHITECTURE.md**:191 documenta `GET /api/mileage-alerts/due` (ruta separada)
- **Realidad**: Implementado como query param `?due=true` en `MileageAlertsController.cs`:26
- **API.md**:334-336 correctamente documenta el query param
- **Fix**: Corregir ARCHITECTURE.md

---

## BAJOS

### 10. Falta `GET /api/suppliers/{id}` en ARCHITECTURE.md
- Existe en `SuppliersController.cs`:29-36 y en API.md:124-125
- ARCHITECTURE.md:135-142 omite el endpoint
- **Fix**: Agregar a ARCHITECTURE.md

### 11. Falta `GET /api/financial-records/{id}` en ARCHITECTURE.md
- Existe en `FinancialRecordsController.cs`:29-36 y en API.md:425-426
- ARCHITECTURE.md:200-206 omite el endpoint
- **Fix**: Agregar a ARCHITECTURE.md

### 12. Response de Register no documentado en API.md
- API.md:39-53 solo muestra Request, sin Response example
- El endpoint retorna el mismo `AuthResponse` que Login
- **Fix**: Agregar response example

### 13. `IsDeleted` no documentado en modelos
- `ServiceOrderItem`, `MileageAlert`, `Notification` tienen `IsDeleted` pero ARCHITECTURE.md no lo lista
- ARCHITECTURE.md:82,84,86
- **Fix**: Agregar a la documentación de tablas

### 14. ServiceVariants usa `IsActive` no `IsDeleted`
- ARCHITECTURE.md:83 lista `IsActive` (correcto)
- Pero la sección 5.8 dice "soft delete" cuando en realidad desactiva (`IsActive = false`)
- **Fix**: Clarificar terminología en docs

### 15. MileageAlerts usa `IsActive` no `IsDeleted`
- `DeleteAsync` en `MileageAlertService.cs`:85 setea `IsActive = false`, no `IsDeleted = true`
- ARCHITECTURE.md:84 lista `IsActive` (correcto) pero endpoint `DELETE` describe "Desactiva la alerta (soft delete)"
- **Fix**: Clarificar que es desactivación lógica, no soft delete tradicional

---

## TRIVIALES

### 16. `POST /api/mileage-alerts/{id}/send` documentado dos veces
- API.md:386-388 (bajo MileageAlerts) y API.md:556-558 (bajo Notificaciones)
- **Fix**: Eliminar duplicado

### 17. Faltan códigos HTTP 204 y 429 en tabla de API.md
- API.md:616-626 no incluye 204 No Content ni 429 Too Many Requests
- Rate limiter configurado en Program.cs:33 con `RejectionStatusCode = 429`
- **Fix**: Agregar a la tabla

### 18. No hay job programado para revisión diaria de alertas
- ARCHITECTURE.md:440 menciona "job programado (o consulta manual)"
- Solo existe la consulta manual via `?due=true`
- **Fix**: Documentar que solo hay consulta manual, o implementar BackgroundService

---

## Resumen

| Prioridad | Cantidad |
|-----------|----------|
| Críticos | 2 |
| Altos | 5 |
| Medios | 2 |
| Bajos | 6 |
| Triviales | 3 |
| **Total** | **18** |
