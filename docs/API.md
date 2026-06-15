# API AutoNex

**Base URL:** `http://localhost:5212`

## Autenticación

Todas las rutas protegidas requieren header:

```text
Authorization: Bearer <token>
```

---

### `POST /api/auth/login`

Público. Inicia sesión y devuelve un JWT.

**Request:**

```json
{
  "email": "admin@autonex.com",
  "password": "Admin123"
}
```

**Response:**

```json
{
  "data": {
    "userId": 1,
    "fullName": "Admin AutoNex",
    "email": "admin@autonex.com",
    "role": "Admin",
    "token": "eyJ..."
  },
  "success": true
}
```

### `POST /api/auth/register`

Requiere rol **Admin**. Crea un nuevo usuario.

**Request:**

```json
{
  "fullName": "Juan Mecánico",
  "email": "juan@autonex.com",
  "password": "Password1",
  "role": "Mechanic",
  "phone": "04121234567"
}
```

**Response:** Misma estructura que Login (`userId`, `fullName`, `email`, `role`, `token`).

**Roles válidos:** `Admin`, `Mechanic`, `Receptionist`

---

## Usuarios

### `GET /api/users`

Lista todos los usuarios. Requiere rol **Admin**.

### `GET /api/users/{id}`

Obtiene un usuario por ID. Requiere rol **Admin**.

### `PUT /api/users/{id}`

Actualiza un usuario. Requiere rol **Admin**.

**Request:**

```json
{
  "fullName": "Juan Mecánico",
  "email": "juan@autonex.com",
  "role": "Mechanic",
  "phone": "04121234567"
}
```

---

## Clientes

### `GET /api/clients`

Lista clientes. Opcional: `?search=nombre` (busca por nombre o teléfono).

### `GET /api/clients/{id}`

Obtiene un cliente con sus vehículos incluidos.

### `POST /api/clients`

Crea un cliente.

**Request:**

```json
{
  "fullName": "Juan Pérez",
  "phone": "04121234567",
  "email": "juan@test.com",
  "address": "Av. Principal, Caracas"
}
```

`email` y `address` son opcionales.

### `PUT /api/clients/{id}`

Actualiza un cliente. Mismos campos que POST.

### `DELETE /api/clients/{id}`

Soft delete del cliente.

---

## Vehículos

### `GET /api/vehicles`

Lista vehículos. Opcional: `?search=placa` (busca por placa o nombre del cliente).

### `GET /api/vehicles/{id}`

Obtiene un vehículo.

### `POST /api/vehicles`

Crea un vehículo asociado a un cliente.

**Request:**

```json
{
  "clientId": 1,
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2020,
  "licensePlate": "ABC123",
  "vin": "1HGBH41JXMN109186"
}
```

`vin` es opcional.

### `PUT /api/vehicles/{id}`

Actualiza un vehículo. Mismos campos que POST (excepto `clientId`).

### `DELETE /api/vehicles/{id}`

Soft delete del vehículo.

---

## Proveedores

### `GET /api/suppliers`

Lista proveedores.

### `GET /api/suppliers/{id}`

Obtiene un proveedor.

### `POST /api/suppliers`

Crea un proveedor.

**Request:**

```json
{
  "name": "Repuestos El Conejo",
  "contactPerson": "Carlos López",
  "phone": "02121234567",
  "email": "carlos@repuestos.com"
}
```

`contactPerson`, `phone` y `email` son opcionales.

### `PUT /api/suppliers/{id}`

Actualiza un proveedor. Mismos campos que POST.

### `DELETE /api/suppliers/{id}`

Soft delete del proveedor.

---

## Consumibles

### `GET /api/consumables`

Lista consumibles. Opcional: `?category=Oil` (filtro por categoría).

**Categorías:** `Oil`, `SparkPlug`, `Coolant`, `Grease`, `BrakeFluid`, `Other`

### `GET /api/consumables/low-stock`

Lista consumibles con stock por debajo del mínimo.

### `GET /api/consumables/{id}`

Obtiene un consumible.

### `POST /api/consumables`

Crea un consumible.

**Request:**

```json
{
  "name": "Aceite 20W50",
  "category": "Oil",
  "stockQuantity": 10,
  "minStock": 2,
  "unitPrice": 25.00,
  "supplierId": 1
}
```

`supplierId` es opcional.

### `PUT /api/consumables/{id}`

Actualiza un consumible. Mismos campos que POST.

### `DELETE /api/consumables/{id}`

Soft delete del consumible.

---

## Categorías de Herramientas

### `GET /api/tool-categories`

Lista categorías. Opcional: `?page=1&pageSize=10`.

### `GET /api/tool-categories/{id}`

Obtiene una categoría por ID.

### `POST /api/tool-categories`

Requiere rol **Admin**. Crea una categoría.

**Request:**

```json
{
  "name": "Gato Hidráulico"
}
```

### `PUT /api/tool-categories/{id}`

Requiere rol **Admin**. Actualiza una categoría. Mismos campos que POST.

### `DELETE /api/tool-categories/{id}`

Requiere rol **Admin**. Soft delete de la categoría.

---

## Herramientas

### `GET /api/tools`

Lista herramientas. Opcional: `?categoryName=Gato&status=Available&page=1&pageSize=10`.

`categoryName` filtra por nombre parcial de la categoría.

**Estados:** `Available`, `Damaged`, `Lost`

### `GET /api/tools/{id}`

Obtiene una herramienta.

### `POST /api/tools`

Requiere rol **Admin**. Crea una herramienta.

**Request:**

```json
{
  "name": "Gato Hidráulico",
  "toolCategoryId": 1,
  "quantity": 2,
  "status": "Available",
  "purchaseDate": "2026-01-15"
}
```

`purchaseDate` es opcional. `toolCategoryId` debe corresponder a una categoría existente en `/api/tool-categories`.

### `PUT /api/tools/{id}`

Requiere rol **Admin**. Actualiza una herramienta. Mismos campos que POST.

### `DELETE /api/tools/{id}`

Requiere rol **Admin**. Soft delete de la herramienta.

---

## Catálogo de Servicios

## Catálogo de Servicios

### `GET /api/services`

Lista servicios del catálogo.

### `GET /api/services/{id}`

Obtiene un servicio.

### `POST /api/services`

Requiere rol **Admin**. Crea un servicio.

**Request:**

```json
{
  "name": "Cambio de Aceite",
  "description": "Cambio de aceite y filtro",
  "defaultPrice": 45.00,
  "recommendedKmInterval": 10000
}
```

`recommendedKmInterval` y `description` son opcionales.

### `PUT /api/services/{id}`

Requiere rol **Admin**. Actualiza un servicio. Mismos campos que POST.

### `DELETE /api/services/{id}`

Requiere rol **Admin**. Soft delete del servicio.

### `GET /api/services/{serviceId}/variants`

Lista las variantes de un servicio.

### `GET /api/services/variants/{id}`

Obtiene una variante por ID.

### `POST /api/services/{serviceId}/variants`

Requiere rol **Admin**. Crea una variante.

**Request:**

```json
{
  "name": "Aceite Sintético 5W-30",
  "description": "Para motores modernos",
  "minKmInterval": 10000,
  "maxKmInterval": 15000,
  "recommendedMonths": 12
}
```

`description` y `recommendedMonths` son opcionales.

### `PUT /api/services/variants/{id}`

Requiere rol **Admin**. Actualiza una variante. Mismos campos que POST.

### `DELETE /api/services/variants/{id}`

Requiere rol **Admin**. Desactiva una variante (soft delete).

---

## Órdenes de Servicio

### `GET /api/service-orders`

Lista órdenes. Filtros opcionales:
`?from=2026-01-01&to=2026-12-31&clientId=1&vehicleId=1&status=Open`

**Estados:** `Open`, `InProgress`, `Completed`, `Cancelled`

### `GET /api/service-orders/{id}`

Obtiene una orden con todos sus items.

### `POST /api/service-orders`

Crea una orden. Descuenta stock de consumibles automáticamente.
Al marcar como `Completed`, actualiza la alerta de kilometraje del vehículo.

**Request:**

```json
{
  "vehicleId": 1,
  "clientId": 1,
  "currentKm": 50000,
  "notes": "Cambio de aceite y filtro",
  "items": [
    {
      "serviceId": 1,
      "serviceVariantId": 1,
      "consumableId": 1,
      "quantity": 1,
      "unitPrice": 35.00
    }
  ]
}
```

`serviceVariantId` y `consumableId` son opcionales.

### `PUT /api/service-orders/{id}`

Actualiza una orden (solo si no está `Completed` o `Cancelled`).
Revierte el stock anterior y vuelve a descontar con los nuevos items.

### `PATCH /api/service-orders/{id}/status`

Cambia el estado de una orden. Al pasar a `Completed`, actualiza automáticamente la alerta de kilometraje del vehículo.

**Request:**

```json
{
  "status": "Completed"
}
```

---

## Alertas de Kilometraje

### `GET /api/mileage-alerts`

Lista todas las alertas activas.

### `GET /api/mileage-alerts?due=true`

Filtra solo las alertas próximas a vencer según la fórmula:
`kmActual + (kmPorSemana × 2) >= kmAlerta`

### `GET /api/mileage-alerts/{id}`

Obtiene una alerta.

**Response:**

```json
{
  "data": {
    "id": 1,
    "vehicleId": 1,
    "vehicleInfo": "Toyota Corolla (ABC123)",
    "lastRecordedKm": 50000,
    "estimatedWeeklyKm": 300,
    "nextAlertKm": 60000,
    "remainingKm": 10000,
    "isDue": false,
    "lastAlertDate": null,
    "isActive": true,
    "createdAt": "2026-06-11T00:00:00Z"
  },
  "success": true,
  "message": "Operación exitosa"
}
```

### `POST /api/mileage-alerts`

Configura una alerta para un vehículo.

**Request:**

```json
{
  "vehicleId": 1,
  "estimatedWeeklyKm": 300
}
```

### `PUT /api/mileage-alerts/{id}`

Actualiza el kilometraje semanal estimado.

**Request:**

```json
{
  "estimatedWeeklyKm": 350
}
```

### `DELETE /api/mileage-alerts/{id}`

Desactiva la alerta (IsActive = false).

### `POST /api/mileage-alerts/{id}/send`

Genera un recordatorio manual. (El envío real por WhatsApp se integrará en etapa posterior).

---

## Finanzas

### `GET /api/financial-records`

Lista registros financieros con filtros opcionales por fecha, tipo y categoría.

**Query Parameters:**

| Parámetro | Tipo | Descripción |
| ----------- | ------ | ------------- |
| `from` | DateTime? | Fecha inicial (inclusive) |
| `to` | DateTime? | Fecha final (inclusive) |
| `type` | string? | `Income` o `Expense` |
| `category` | string? | `Services`, `Suppliers`, `Rent`, `Payroll`, `Utilities`, `Other` |

**Response:**

```json
{
  "data": [
    {
      "id": 1,
      "type": "Income",
      "category": "Services",
      "amount": 450.00,
      "description": "Mano de obra cambio de aceite",
      "date": "2026-06-11T00:00:00Z",
      "userId": 1,
      "userName": "Admin AutoNex",
      "createdAt": "2026-06-11T00:00:00Z"
    }
  ],
  "success": true,
  "message": "Operación exitosa"
}
```

### `GET /api/financial-records/{id}`

Obtiene un registro financiero por ID.

### `POST /api/financial-records`

Crea un registro financiero (ingreso o egreso).

**Request:**

```json
{
  "type": "Income",
  "category": "Services",
  "amount": 450.00,
  "description": "Mano de obra cambio de aceite",
  "date": "2026-06-11T00:00:00Z",
  "userId": 1
}
```

### `PUT /api/financial-records/{id}`

Actualiza un registro financiero. Mismos campos que POST (excepto `userId`).

### `DELETE /api/financial-records/{id}`

Soft delete del registro financiero.

### `GET /api/financial-records/summary`

Resumen de ingresos vs egresos en un período.

**Query Parameters:**

| Parámetro | Tipo | Descripción |
| ----------- | ------ | ------------- |
| `from` | DateTime? | Fecha inicial |
| `to` | DateTime? | Fecha final |

**Response:**

```json
{
  "data": {
    "totalIncome": 5000.00,
    "totalExpenses": 3200.00,
    "balance": 1800.00,
    "incomeCount": 15,
    "expenseCount": 8
  },
  "success": true,
  "message": "Operación exitosa"
}
```

### `GET /api/financial-records/by-category`

Registros agrupados por categoría, ordenados por monto descendente.

**Query Parameters:**

| Parámetro | Tipo | Descripción |
| ----------- | ------ | ------------- |
| `from` | DateTime? | Fecha inicial |
| `to` | DateTime? | Fecha final |

**Response:**

```json
{
  "data": [
    {
      "category": "Services",
      "totalAmount": 5000.00,
      "count": 15
    },
    {
      "category": "Suppliers",
      "totalAmount": 2000.00,
      "count": 5
    }
  ],
  "success": true,
  "message": "Operación exitosa"
}
```

---

## Notificaciones

### `GET /api/notifications`

Historial de notificaciones enviadas.

**Query Parameters:**

| Parámetro | Tipo | Descripción |
| ----------- | ------ | ------------- |
| `clientId` | int? | Filtrar por cliente |
| `vehicleId` | int? | Filtrar por vehículo |
| `status` | string? | `Pending`, `Sent` o `Failed` |

**Response:**

```json
{
  "data": [
    {
      "id": 1,
      "clientId": 1,
      "clientName": "Juan Pérez",
      "vehicleId": 1,
      "vehicleInfo": "Toyota Corolla (ABC123)",
      "type": "WhatsApp",
      "recipient": "+521234567890",
      "message": "Recordatorio: El vehículo Toyota Corolla (ABC123) requiere atención...",
      "sentAt": "2026-06-11T12:00:00Z",
      "status": "Sent",
      "createdAt": "2026-06-11T12:00:00Z"
    }
  ],
  "success": true,
  "message": "Operación exitosa"
}
```

### `GET /api/notifications/{id}`

Obtiene una notificación por ID.

### `POST /api/notifications/send-whatsapp`

Envía una notificación manual por WhatsApp.

**Request:**

```json
{
  "clientId": 1,
  "vehicleId": 1,
  "type": "WhatsApp",
  "recipient": "+521234567890",
  "message": "Recordatorio: Su vehículo requiere mantenimiento."
}
```

---

## Movimientos de Inventario

### `GET /api/inventory-movements`

Historial de movimientos de inventario (entradas/salidas de consumibles y herramientas).

**Query Parameters:**

| Parámetro | Tipo | Descripción |
| ----------- | ------ | ------------- |
| `consumableId` | int? | Filtrar por consumible |
| `toolId` | int? | Filtrar por herramienta |
| `page` | int? | Número de página |
| `pageSize` | int? | Elementos por página |

**Response:**

```json
{
  "data": [
    {
      "id": 1,
      "consumableId": 1,
      "consumableName": "Aceite 5W-30",
      "toolId": null,
      "toolName": null,
      "movementType": "Out",
      "quantity": 2,
      "reference": "ServiceOrder",
      "referenceId": 5,
      "notes": "Descuento por orden de servicio",
      "createdAt": "2026-06-11T00:00:00Z"
    }
  ],
  "success": true,
  "message": "Operación exitosa"
}
```

### `GET /api/inventory-movements/{id}`

Obtiene un movimiento por ID.

---

## Infraestructura

### `GET /health`

Health check del servicio. Retorna `200 OK` si la API está operativa.

### Paginación

Todos los endpoints de listado aceptan los siguientes query parameters:

| Parámetro | Tipo | Default | Descripción |
| ----------- | ------ | --------- | ------------- |
| `page` | int | 1 | Número de página |
| `pageSize` | int | 20 | Elementos por página (máx. 100) |

**Response paginado:**

```json
{
  "data": {
    "items": [ ... ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "success": true,
  "message": "Operación exitosa"
}
```

---

## Formato de respuesta estándar

**Éxito:**

```json
{
  "data": { ... },
  "success": true,
  "message": "Operación exitosa"
}
```

**Error:**

```json
{
  "data": null,
  "success": false,
  "message": "Descripción del error",
  "errors": {
    "fullName": ["El nombre es obligatorio"]
  }
}
```

**Códigos HTTP:**

| Código | Significado |
| -------- | ------------- |
| 200 | OK |
| 201 | Created (POST) |
| 400 | Bad Request (validación) |
| 401 | Unauthorized (token faltante/inválido) |
| 403 | Forbidden (rol sin permiso) |
| 204 | No Content (DELETE) |
| 404 | Not Found |
| 409 | Conflict (ej. email duplicado, alerta ya existe) |
| 415 | Unsupported Media Type (falta Content-Type) |
| 429 | Too Many Requests (rate limit excedido) |
