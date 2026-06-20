# BCV — Flujo de Tasas de Cambio

## Ciclo de vida de un boletín

```
  BcvFetchJob      Usuario        BcvActivateJob      Consulta
 (M-F 4:10pm)   (click autorizar)  (00:00 VET)
      │               │                │
      ▼               ▼                ▼
   ┌───────┐      ┌──────────┐     ┌───────────┐     ┌────────┐
   │ Draft │ ───→ │Authorized│ ──→ │ Published │ ──→ │Historical│
   └───────┘      └──────────┘     └───────────┘     └────────┘
      ↑                                               ↑
      │                                      BcvActivateJob
  BcvRetryJob                               (reemplaza anterior)
  (c/10 min)
```

## Jobs programados

| Job | Llave | Schedule (VET) | Responsabilidad |
|---|---|---|---|
| **BcvFetchJob** | `bcv-fetch` | `0 10 16 ? * MON-FRI` (Lun–Vie 4:10 PM) | Scrapea BCV, crea Draft si hay tasas nuevas, desactiva `bcv_retry_enabled` |
| **BcvActivateJob** | `bcv-activate` | `0 0 0 ? * *` (00:00 todos los días) | Mueve Authorized → Published, anterior Published → Historical, activa `bcv_retry_enabled` |
| **BcvRetryJob** | `bcv-retry` | `0 0/10 4-23 ? * *` (cada 10 min, 4 AM – 11:50 PM) | Verifica si hay Draft/Published para hoy; si no, fetch BCV; si inserta Draft desactiva `bcv_retry_enabled` |

## Settings

| Key | Valor inicial | Controla |
|---|---|---|
| `bcv_auto_consult` | `false` | Habilita el fetch automático (`BcvFetchJob`) y el retry. El toggle en UI sincroniza `bcv_retry_enabled` al mismo valor. |
| `bcv_update_cron` | `10 16 * * 1-5` | Referencia de la expresión cron (no usada en código) |
| `bcv_retry_enabled` | `false` | Habilita el reintento cada 10 min (`BcvRetryJob`). Se activa/desactiva automáticamente. |

## Flujo detallado

### Lunes a Viernes

```
Horario     Suceso
─────────────────────────────────────────────────────
4:10 PM     BcvFetchJob: scrapea BCV.
            └─ Si hay tasas → crea Draft + desactiva bcv_retry_enabled = false
            └─ Si BCV no publicó → no hace nada

4:20–11:50  BcvRetryJob: cada 10 min (con ventana weekday)
            └─ Si bcv_retry_enabled != true → skip
            └─ Si antes de 4:20 PM → skip (OutsideWindow)
            └─ Si ya hay Published/Draft para hoy → skip
            └─ Si no → fetch BCV
                ├─ Éxito (inserta Draft) → desactiva bcv_retry_enabled
                └─ Falla → sigue en próximo ciclo

00:00       BcvActivateJob: activa boletín
            └─ Busca el Authorized más reciente
            └─ Si su ValueDate es futuro → skip (no activa)
            └─ Si ValueDate == hoy o pasado:
                ├─ Mueve Published actual → Historical
                ├─ Authorized → Published
                └─ Activa bcv_retry_enabled = true (prepara para siguiente fetch)
```

### Fin de semana (Sábado, Domingo)

```
Horario     Suceso
─────────────────────────────────────────────────────
00:00       BcvActivateJob: NO activa si ValueDate es futuro
            └─ Ejemplo: Sábado → Authorized con ValueDate = Lunes → skip
            └─ El Published con ValueDate = Viernes sigue vigente

04:00–23:50 BcvRetryJob: cada 10 min (sin restricción weekday)
            └─ Misma lógica: verifica Published/Draft para hoy
            └─ Si el BCV publicó en fin de semana (atípico) → lo captura
            └─ Si no hay publicación → sigue intentando
```

### Consideraciones especiales

- **Viernes**: BCV publica la tasa del **lunes**. El Authorized con `ValueDate = Lunes` no se activa hasta el lunes a medianoche.
- **Sábado/Domingo**: la tasa vigente es la del viernes (`ValueDate = Viernes`). `BcvActivateJob` respeta `ValueDate` futuro y no la reemplaza.
- **Published tardío**: si el BCV no publica el viernes, el retry sigue cada 10 min sábado y domingo hasta capturar la publicación.
- **`live/{currency}` endpoint**: siempre limpia el caché antes de responder, asegurando datos frescos.

## Notificaciones SignalR

| Evento | Grupo | Cuándo se emite | Payload |
|---|---|---|---|
| `ExchangeRatePublished` | `exchange-updates` | Cuando se inserta un Draft exitoso (fetch o retry) | `{ newsletterId }` |
| `ExchangeRateAuthorized` | `exchange-updates` | Cuando usuario autoriza un Draft | `{ newsletterId }` |
| `ExchangeRateInEffect` | `exchange-updates` | Cuando `BcvActivateJob` publica un boletín | `{ newsletterId }` |
| `PublicExchangeRate` | `exchange-rates-public` | Cuando `BcvActivateJob` publica un boletín (solo USD) | `{ currency, value }` |

## Toggle Auto BCV (UI)

El botón **Auto BCV** controla `bcv_auto_consult`. Al activarlo/desactivarlo:

1. Cambia `bcv_auto_consult` al valor opuesto
2. Sincroniza `bcv_retry_enabled` al mismo valor
3. **No** dispara fetch inmediato (el fetch ocurre en su horario programado)

## Seguridad

- El `ExchangeRateHub` requiere política `ExchangeRatesUpdates` con claim `permission: autoupdate-bcv`
- Todos los usuarios autenticados tienen este claim (ver `AuthService.GenerateToken()`)

## Fetch Logs

Cada intento de fetch (exitoso o fallido) se registra en `bcv_fetch_logs` con:

| Campo | Descripción |
|---|---|
| `value_date` | Fecha de la tasa obtenida (o UTC si falló) |
| `rates_json` | JSON con las tasas obtenidas (solo si éxito) |
| `is_success` | `true` o `false` |
| `action` | `Auto_Inserted`, `Auto_Skipped_AlreadyPublished`, `Auto_Skipped_AlreadyDraft`, `Auto_Failed`, `Retry_Inserted`, `Retry_Skipped_AlreadyPublished`, `Retry_Skipped_AlreadyDraft`, `Retry_Skipped_OutsideWindow`, `Retry_Failed` |
| `fetched_by` | `"Auto"` (BcvFetchJob) o `"Retry"` (BcvRetryJob) |
| `error` | Mensaje de error (solo si falló) |
| `fetched_at` | Timestamp UTC |
