# AutoNex — Sistema de Gestión para Taller Mecánico

API REST para la administración integral de un taller mecánico. Desarrollada en ASP.NET Core 10 con PostgreSQL.

## Módulos del Sistema

| Módulo | Descripción |
|---|---|
| **Usuarios y Roles** | Autenticación JWT con roles: Admin, Mecánico, Recepcionista |
| **Clientes** | Gestión de datos personales de los dueños de vehículos |
| **Vehículos** | Registro de vehículos asociados a clientes |
| **Proveedores** | Catálogo de proveedores de insumos y herramientas |
| **Consumibles** | Inventario de aceites, bujías, refrigerantes, etc. con control de stock mínimo |
| **Herramientas** | Inventario de herramientas de trabajo con seguimiento de estado |
| **Órdenes de Servicio** | Registro de atención: vehículo, servicios realizados, consumibles usados, kilometraje |
| **Alertas por Kilometraje** | Cálculo de desgaste estimado y recordatorios automáticos vía WhatsApp |
| **Finanzas** | Control de ingresos y egresos del taller |
| **Notificaciones** | Integración con Twilio/WhatsApp API para recordatorios |

## Stack Tecnológico

- **Runtime:** .NET 10 / ASP.NET Core
- **Base de Datos:** PostgreSQL + Entity Framework Core
- **Autenticación:** JWT Bearer con roles
- **API:** RESTful (Controllers)
- **Notificaciones:** Twilio API / WhatsApp Business API
- **Documentación:** OpenAPI / Swagger

## Etapas del Proyecto

1. **Fundación** — DB, Auth, CRUD de Clientes/Vehículos/Proveedores/Usuarios
2. **Inventarios** — Consumibles y Herramientas
3. **Órdenes de Servicio** — Atención de vehículos y consumo de inventario
4. **Alertas de Kilometraje** — Cálculo y recordatorios
5. **Finanzas** — Ingresos, egresos y dashboards
6. **WhatsApp & Notificaciones** — Integración de mensajería

Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para documentación técnica detallada.
