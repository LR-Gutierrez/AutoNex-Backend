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
