# API AutoNex

**Base URL:** `http://localhost:5212`

## Autenticación

Todas las rutas protegidas requieren header:
```
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

**Roles válidos:** `Admin`, `Mechanic`, `Receptionist`

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

## Herramientas

### `GET /api/tools`
Lista herramientas. Opcional: `?category=Wrench&status=Available`.

**Categorías:** `Jack`, `Wrench`, `Ratchet`, `Screwdriver`, `Hammer`, `Other`

**Estados:** `Available`, `Damaged`, `Lost`

### `GET /api/tools/{id}`
Obtiene una herramienta.

### `POST /api/tools`
Crea una herramienta.

**Request:**
```json
{
  "name": "Gato Hidráulico",
  "category": "Jack",
  "quantity": 2,
  "status": "Available",
  "purchaseDate": "2026-01-15"
}
```
`purchaseDate` es opcional.

### `PUT /api/tools/{id}`
Actualiza una herramienta. Mismos campos que POST.

### `DELETE /api/tools/{id}`
Soft delete de la herramienta.

---

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
  "defaultPrice": 45.00
}
```

### `PUT /api/services/{id}`
Requiere rol **Admin**. Actualiza un servicio.

### `DELETE /api/services/{id}`
Requiere rol **Admin**. Soft delete del servicio.

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
|--------|-------------|
| 200 | OK |
| 201 | Created (POST) |
| 400 | Bad Request (validación) |
| 401 | Unauthorized (token faltante/inválido) |
| 403 | Forbidden (rol sin permiso) |
| 404 | Not Found |
| 409 | Conflict (ej. email duplicado) |
| 415 | Unsupported Media Type (falta Content-Type) |
