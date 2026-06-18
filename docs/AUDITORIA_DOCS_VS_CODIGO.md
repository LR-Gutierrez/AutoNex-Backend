# Auditoría: Documentación vs Código

Fecha original: 2026-06-11
Última revisión: 2026-06-18

---

## Resumen

| Prioridad | Original | Resueltos | Pendientes |
|-----------|----------|-----------|------------|
| Críticos | 2 | 2 | 0 |
| Altos | 5 | 5 | 0 |
| Medios | 2 | 2 | 0 |
| Bajos | 6 | 6 | 0 |
| Triviales | 3 | 3 | 0 |
| **Total** | **18** | **18** | **0** |

> **Nota:** Durante la revisión del 2026-06-18 se resolvieron además issues de calidad de código: `CancellationToken` en toda la capa de servicios, `AsNoTracking()` en consultas de solo lectura, filtro `IsDeleted` explícito, normalización de emails, eliminación de usuarios, separación de `TwilioSettings` en archivo propio, y documentación faltante.

---

## CRÍTICOS

### ~~1. Endpoints de Usuarios documentados pero NO implementados~~ ✅ RESUELTO
- `UsersController.cs`, `IUserService`, `UserService` creados en commit `b824005`
- ARCHITECTURE.md actualizado

### ~~2. Tabla InventoryMovements documentada pero NO implementada~~ ✅ RESUELTO
- `InventoryMovement.cs`, controlador, servicio y DTOs creados en commit `7266d5d`

---

## ALTOS

### ~~3. Register devuelve 200 en vez de 201 Created~~ ✅ RESUELTO
- Corregido en commit `2229869`

### ~~4. DELETE devuelve 200 en vez de 204 No Content~~ ✅ RESUELTO
- Todos los DELETE ahora retornan `NoContent()` — commit `2229869`

### ~~5. Vehicle `/{id}` no incluye historial de órdenes~~ ✅ RESUELTO
- `VehicleResponse` incluye `ServiceOrders` con datos — commit `c81dcd9`

### ~~6. RequestLoggingMiddleware documentado pero no existe~~ ✅ RESUELTO
- Implementado en `Middleware/RequestLoggingMiddleware.cs`

### ~~7. Falta autorización por roles en Suppliers, Consumables, Tools~~ ✅ RESUELTO
- `[Authorize(Roles = "Admin")]` agregado en endpoints de escritura — commit `b824005`

---

## MEDIOS

### ~~8. Route `api/financial-records` usa kebab en docs, PascalCase en código~~ ✅ RESUELTO
- Route explícita `[Route("api/financial-records")]` agregada

### ~~9. MileageAlerts `/due` como ruta separada en ARCHITECTURE.md~~ ✅ RESUELTO
- Implementado como query param `?due=true`, documentado correctamente

---

## BAJOS

### ~~10. Falta `GET /api/suppliers/{id}` en ARCHITECTURE.md~~ ✅ RESUELTO
- Endpoint agregado a ARCHITECTURE.md

### ~~11. Falta `GET /api/financial-records/{id}` en ARCHITECTURE.md~~ ✅ RESUELTO
- Endpoint agregado a ARCHITECTURE.md

### ~~12. Response de Register no documentado en API.md~~ ✅ RESUELTO
- Response example agregado

### ~~13. `IsDeleted` no documentado en modelos~~ ✅ RESUELTO
- Actualizado en ARCHITECTURE.md (ServiceOrderItems, MileageAlert, Notification incluyen IsDeleted)

### ~~14. ServiceVariants usa `IsActive` no `IsDeleted`~~ ✅ RESUELTO
- ServiceVariant eliminado del modelo. Servicios ahora usan `IsDeleted` estándar.

### ~~15. MileageAlerts usa `IsActive` no `IsDeleted`~~ ✅ RESUELTO
- Documentación clarifica que es desactivación lógica con `IsActive`

---

## TRIVIALES

### ~~16. `POST /api/mileage-alerts/{id}/send` documentado dos veces~~ ✅ RESUELTO
- Eliminada entrada duplicada en API.md

### ~~17. Faltan códigos HTTP 204 y 429 en tabla de API.md~~ ✅ RESUELTO
- Ya incluidos en la tabla de códigos HTTP

### ~~18. No hay job programado para revisión diaria de alertas~~ ✅ RESUELTO
- `MileageAlertBackgroundService` implementado (cada hora) — commit `c81dcd9`

---

## Pendientes remanentes

Todos los ítems identificados en la auditoría original han sido resueltos. ✅

Todos los issues de calidad de código identificados en la revisión del 2026-06-18 también han sido corregidos. ✅
