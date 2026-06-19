# Flujo de Status — Tasas de Cambio BCV

## Mapa de Estados

```
                        ┌──────────────────────────────────────────────┐
                        │           STATUS_DRAFT = 1                   │
                        │           "Pending to authorize"             │
                        │                                              │
                        │  ● Creado automáticamente por BcvFetchJob    │
                        │  ● Creado manualmente vía formulario create  │
                        │  ● Es el ÚNICO status que se puede editar    │
                        │  ● Se puede eliminar/desactivar              │
                        └───────────────────┬──────────────────────────┘
                                            │
                                            │  Acción: authorizeRate()
                                            │  POST /exchange-rates/{id}/authorize
                                            ▼
                        ┌──────────────────────────────────────────────┐
                        │         STATUS_AUTHORIZED = 2                │
                        │           "Authorized"                       │
                        │                                              │
                        │  ● El usuario con permiso "authorize"        │
                        │    autorizó manualmente el boletín           │
                        │  ● Queda en espera de las 00:00 VET          │
                        │    para entrar en vigencia                   │
                        │  ● Ya NO se puede editar                     │
                        │  ● Broadcast: ExchangeRateAuthorized event   │
                        └───────────────────┬──────────────────────────┘
                                            │
                                            │  Acción: bcv:activate-rates
                                            │  (Schedule diario 00:00 VET)
                                            ▼
          ┌────────────────────────────────────────────────────────────────┐
          │                     STATUS_PUBLISHED = 3                       │
          │                       "Valid / In effect"                      │
          │                                                                │
          │  ● Es la tasa VIGENTE que usa el sistema                      │
          │  ● ExchangeRateService.getCurrentNewsletter() la retorna      │
          │  ● Todos los módulos (checkout, ventas) consumen esta tasa    │
          │  ● Broadcast: ExchangeRateInEffect + PublicExchangeRate       │
          │  ● Se notifica a usuarios con permiso "autoupdate-bcv"        │
          └───────────────────┬────────────────────────────────────────────┘
                              │
                              │  Acción: BcvActivateJob detecta un
                              │  nuevo STATUS_AUTHORIZED → lo pasa a
                              │  STATUS_PUBLISHED, y el actual pasa a:
                              ▼
          ┌────────────────────────────────────────────────────────────────┐
          │                   STATUS_HISTORICAL = 4                        │
          │                      "Historical"                              │
          │                                                                │
          │  ● Ya no está vigente                                          │
          │  ● Solo queda como registro de auditoría                       │
          │  ● Se conserva la observación con la fecha de revocación       │
          └────────────────────────────────────────────────────────────────┘
```

---

## Valores y Significado

| Constante           | Valor | Etiqueta UI          | Color       | Descripción                                  |
| ------------------- | ----- | -------------------- | ----------- | -------------------------------------------- |
| `STATUS_DRAFT`      | 1     | Pending to authorize | 🟡 Amarillo | Recién creado, esperando autorización manual |
| `STATUS_AUTHORIZED` | 2     | Authorized           | 🔵 Azul     | Autorizado por un usuario, espera las 00:00  |
| `STATUS_PUBLISHED`  | 3     | Valid / In effect    | 🟢 Verde    | Vigente, usado por todo el sistema           |
| `STATUS_HISTORICAL` | 4     | Historical           | ⚪ Gris     | Reemplazado, solo auditoría                  |

Adicionalmente, si `is_active = false` → **Inactive** (🔴 Rojo), independientemente del status.

---

## Transiciones Permitidas

```
Draft (1) ────→ Authorized (2)     ✓  (vía authorizeRate)
Draft (1) ────→ Inactive           ✓  (vía disable)
Draft (1) ────→ Draft (1) edit     ✓  (vía update)

Authorized (2) ──→ Published (3)   ✓  (vía bcv:activate-rates a las 00:00)
Authorized (2) ──→ Inactive        ✓  (vía disable)

Published (3) ───→ Historical (4)  ✓  (vía bcv:activate-rates cuando llega uno nuevo)
Published (3) ───→ Inactive        ✓  (vía disable)

Historical (4) ──→ Inactive        ✓  (vía disable)
```

**Transiciones NO permitidas (validación en código):**

| Transición             | Por qué                    |
| ---------------------- | -------------------------- |
| Authorized → Draft     | No se puede "desautorizar" |
| Published → Draft      | Ya fue vigente             |
| Published → Authorized | No tiene sentido           |
| Historical → Published | Ya fue reemplazado         |
| Historical → Draft     | No se puede revertir       |

---

## Código Relevante

### Definición de constantes (`CurrencyNewsletter.php:38-41`)

```php
const STATUS_DRAFT      = 1;
const STATUS_AUTHORIZED = 2;
const STATUS_PUBLISHED  = 3;
const STATUS_HISTORICAL = 4;
```

### Mapeo a etiquetas UI (`CurrencyNewsletter.php:62-73`)

```php
public function getStatusKeyAttribute(): string
{
    if (!$this->is_active) return 'inactive';

    return match ((int) $this->status) {
        self::STATUS_DRAFT      => 'pending',      // "Pending to authorize"
        self::STATUS_AUTHORIZED => 'queued',        // "Authorized"
        self::STATUS_PUBLISHED  => 'active',        // "Valid"
        self::STATUS_HISTORICAL => 'historical',    // "Historical"
        default                 => 'unknown',
    };
}
```

### Render de badges en el DataTable (`ExchangeRateController.php:63-70`)

```php
'match ($row->status_key) {
    'active'     => '<span class="... bg-green-500 ...">Valid</span>',
    'queued'     => '<span class="... bg-blue-600 ...">Authorized</span>',
    'historical' => '<span class="... bg-gray-500 ...">Historical</span>',
    'pending'    => '<span class="... bg-yellow-500 ...">Pending to authorize</span>',
    default      => '<span class="... bg-red-500 ...">Inactive</span>',
}'
```

### Consulta de tasa vigente (`ExchangeRateService.php:27-31`)

```php
CurrencyNewsletter::query()
    ->where('status', CurrencyNewsletter::STATUS_PUBLISHED)  // ← Solo Status 3
    ->where('is_active', true)                                // ← Y activo
    ->orderBy('value_date', 'desc')
    ->first();
```

### Autorización (`ExchangeRateController.php:351-387`)

```php
// Solo se puede autorizar un Draft (Status 1)
if ($newsletter->status !== CurrencyNewsletter::STATUS_DRAFT) {
    return response()->json([...error...], 422);
}

$newsletter->status = CurrencyNewsletter::STATUS_AUTHORIZED;  // → Status 2
```

### Activación automática a las 00:00 (`ActivateAuthorizedRates.php:35-64`)

```php
// 1. Buscar el Status 2 más reciente
$nextBulletin = CurrencyNewsletter::where('status', STATUS_AUTHORIZED)
    ->orderBy('value_date', 'desc')->first();

// 2. El Status 3 actual → Status 4 (Historical)
CurrencyNewsletter::where('status', STATUS_PUBLISHED)
    ->update(['status' => STATUS_HISTORICAL]);

// 3. El Status 2 encontrado → Status 3 (Published)
$nextBulletin->update(['status' => STATUS_PUBLISHED]);
```

---

## Eventos Disparados por cada Transición

| Transición                  | Evento SignalR                                | Efecto                                            |
| --------------------------- | --------------------------------------------- | ------------------------------------------------- |
| Draft → Authorized          | `ExchangeRateAuthorized`                      | El frontend recarga la tabla                      |
| Authorized → Published      | `ExchangeRateInEffect` + `PublicExchangeRate` | Se actualiza la tasa en todo el sistema           |
| Published → Historical      | (implícito en el mismo evento anterior)       | La anterior deja de ser vigente                   |
| Fetch automático crea Draft | `ExchangeRatePublished`                       | Aparece notificación de "pendiente por autorizar" |

---

## Notas Importantes

1. **Un boletín en Status 1 (Draft) es el único que se puede editar**, porque el formulario `edit.blade.php` solo carga registros editables. La lógica en la columna `actions` del DataTable oculta los botones de edición para status ≥ 2:

   ```php
   if ($row->status >= CurrencyNewsletter::STATUS_AUTHORIZED) {
       // Muestra solo texto "In effect" o "Auditable", sin botones
       return '<span class="...">In effect</span>';
   }
   // Si es Draft: muestra botones de editar, autorizar, eliminar
   ```

2. **El flag `is_active`** actúa como "soft delete" o "desactivación manual". Si un boletín está `is_active = false`, se considera **Inactive** sin importar su status. La mayoría de las queries del sistema filtran por `is_active = true`.

3. **El `bcv:activate-rates` se ejecuta una sola vez al día (00:00 VET).** Si hay múltiples boletines Authorized, solo toma el más reciente (`orderBy('value_date', 'desc')->first()`).

4. **El sistema puede tener un solo Status 3 (Published) vigente a la vez.** Cuando se activa uno nuevo, el anterior pasa automáticamente a Historical.

5. **La tasa que consumen los módulos** (checkout, ventas, etc.) es SIEMPRE la del newsletter con `status = 3, is_active = true`, cacheada por 1 hora en `ExchangeRateService`.
