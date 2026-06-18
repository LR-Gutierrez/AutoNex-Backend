# Análisis y Directrices — Frontend AutoNex

## 1. Descripción General

Este documento establece las directrices arquitectónicas, técnicas y de implementación para el desarrollo del frontend del sistema **AutoNex**, un aplicativo web para la gestión integral de talleres mecánicos automotrices. El frontend consumirá la API REST construida en ASP.NET Core 10 y estará orientado a tres perfiles de usuario: **Admin**, **Mechanic** y **Receptionist**, cada uno con privilegios específicos sobre los módulos del sistema.

El objetivo del presente análisis es definir el stack tecnológico, la estructura del proyecto, el mapa de navegación, el flujo de autenticación, las convenciones de código y el plan de implementación por etapas, de modo que sirva como guía única y vinculante para todo el desarrollo frontend.

---

## 2. Stack Tecnológico

| Componente | Tecnología | Versión | Justificación |
|---|---|---|---|---|
| Framework | Ionic (Angular) | 8+ | UI cross-platform nativa, mobile-first, componente de UI completo (listas, tabs, modales, cards) con diseño Material adaptativo |
| Lenguaje | TypeScript | 5.7+ | Tipado estricto para modelar DTOs del backend, reduce errores en tiempo de compilación |
| Estilos | Tailwind CSS | 3.4+ | Utility-first, zero runtime, complementa estilos de Ionic sin fricción |
| Charts | ngx-charts | 20+ | Visualización financiera basada en D3, integración nativa con Angular Signals |
| Fechas | date-fns | 3+ | Librería ligera de manipulación de fechas, árbol de importación reducido |
| Formularios | Angular Reactive Forms | — | Sistema oficial de formularios reactivos con validación síncrona/asíncrona |
| Peticiones HTTP | Angular HttpClient | — | Cliente HTTP oficial con soporte para interceptors y tipado genérico |
| UI Components | Ionic UI Components | — | Componentes nativos: `ion-list`, `ion-item`, `ion-modal`, `ion-toast`, `ion-searchbar`, `ion-select`, etc. |
| Nativo | Capacitor | 6+ | Acceso a APIs nativas (cámara, almacenamiento, notificaciones push) sin capa intermedia |
| Linting/Formateo | ESLint + Prettier | — | Reglas de calidad y formateo automático, alineado al ecosistema Angular |

---

## 3. Estructura del Proyecto

```
frontend/
├── package.json
├── tailwind.config.js
├── tsconfig.json
├── tsconfig.app.json
├── angular.json
├── ionic.config.json
├── capacitor.config.ts
├── src/
│   ├── index.html
│   ├── main.ts
│   ├── theme/
│   │   ├── variables.scss          ← Ionic CSS custom properties
│   │   └── core.css                 ← Tailwind directives + overrides
│   ├── global.scss                  ← Estilos globales Ionic + Tailwind
│   ├── app/
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   ├── app.routes.ts
│   │   ├── layouts/
│   │   │   ├── auth-layout.component.ts
│   │   │   └── dashboard-layout.component.ts
│   │   ├── core/
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── services/
│   │   │   │   ├── api.service.ts
│   │   │   │   ├── auth.service.ts
│   │   │   │   └── auth-state.service.ts
│   │   │   └── models/
│   │   │       ├── api-response.model.ts
│   │   │       ├── auth.model.ts
│   │   │       ├── user.model.ts
│   │   │       ├── client.model.ts
│   │   │       ├── vehicle.model.ts
│   │   │       ├── supplier.model.ts
│   │   │       ├── consumable.model.ts
│   │   │       ├── tool.model.ts
│   │   │       ├── service.model.ts
│   │   │       ├── service-order.model.ts
│   │   │       ├── mileage-alert.model.ts
│   │   │       ├── financial-record.model.ts
│   │   │       ├── notification.model.ts
│   │   │       └── inventory-movement.model.ts
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   │   ├── login.component.ts
│   │   │   │   │   ├── login.component.html
│   │   │   │   │   └── login.service.ts
│   │   │   │   └── register/
│   │   │   │       ├── register.component.ts
│   │   │   │       ├── register.component.html
│   │   │   │       └── register.service.ts
│   │   │   ├── dashboard/
│   │   │   │   ├── dashboard.component.ts
│   │   │   │   └── dashboard.component.html
│   │   │   ├── clients/
│   │   │   │   ├── client-list/
│   │   │   │   ├── client-form/
│   │   │   │   └── client-detail/
│   │   │   ├── vehicles/
│   │   │   │   ├── vehicle-list/
│   │   │   │   ├── vehicle-form/
│   │   │   │   └── vehicle-detail/
│   │   │   ├── suppliers/
│   │   │   │   ├── supplier-list/
│   │   │   │   └── supplier-form/
│   │   │   ├── consumables/
│   │   │   │   ├── consumable-list/
│   │   │   │   ├── low-stock/
│   │   │   │   └── consumable-form/
│   │   │   ├── tools/
│   │   │   │   ├── tool-list/
│   │   │   │   └── tool-form/
│   │   │   ├── services/
│   │   │   │   ├── service-list/
│   │   │   │   ├── service-form/
│   │   │   │   └── service-detail/
│   │   │   ├── service-orders/
│   │   │   │   ├── service-order-list/
│   │   │   │   ├── service-order-form/
│   │   │   │   └── service-order-detail/
│   │   │   ├── mileage-alerts/
│   │   │   │   ├── mileage-alert-list/
│   │   │   │   └── mileage-alert-form/
│   │   │   ├── financial-records/
│   │   │   │   ├── financial-record-list/
│   │   │   │   ├── financial-record-form/
│   │   │   │   └── financial-summary/
│   │   │   ├── notifications/
│   │   │   │   └── notification-list/
│   │   │   ├── users/
│   │   │   │   └── user-list/
│   │   │   └── inventory-movements/
│   │   │       └── inventory-movement-list/
│   │   └── shared/
│   │       ├── components/
│   │       │   ├── empty-state/
│   │       │   ├── loading-spinner/
│   │       │   └── status-badge/
│   │       ├── pipes/
│   │       │   ├── enum-label.pipe.ts
│   │       │   ├── currency-formatter.pipe.ts
│   │       │   └── date-format.pipe.ts
│   │       └── utils/
│   │           ├── pagination-helper.ts
│   │           └── form-validators.ts
│   └── environments/
│       ├── environment.ts
│       └── environment.prod.ts
```

---

## 4. Mapa de Rutas

| Ruta | Componente | Layout | Guard | Rol |
|---|---|---|---|---|
| `/auth/login` | `LoginComponent` | `AuthLayout` | — | Público |
| `/auth/register` | `RegisterComponent` | `AuthLayout` | `AuthGuard` | Admin |
| `/dashboard` | `DashboardComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/clients` | `ClientListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/clients/new` | `ClientFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/clients/:id` | `ClientDetailComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/clients/:id/edit` | `ClientFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/vehicles` | `VehicleListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/vehicles/new` | `VehicleFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/vehicles/:id` | `VehicleDetailComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/vehicles/:id/edit` | `VehicleFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/suppliers` | `SupplierListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/suppliers/new` | `SupplierFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/suppliers/:id/edit` | `SupplierFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/consumables` | `ConsumableListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/consumables/low-stock` | `LowStockComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/consumables/new` | `ConsumableFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/consumables/:id/edit` | `ConsumableFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/tools` | `ToolListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/tools/new` | `ToolFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/tools/:id/edit` | `ToolFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/services` | `ServiceListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/services/new` | `ServiceFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/services/:id` | `ServiceDetailComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/services/:id/edit` | `ServiceFormComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/service-orders` | `ServiceOrderListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/service-orders/new` | `ServiceOrderFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/service-orders/:id` | `ServiceOrderDetailComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/service-orders/:id/edit` | `ServiceOrderFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/mileage-alerts` | `MileageAlertListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/mileage-alerts/new` | `MileageAlertFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/mileage-alerts/:id/edit` | `MileageAlertFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/financial-records` | `FinancialRecordListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/financial-records/summary` | `FinancialSummaryComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/financial-records/new` | `FinancialRecordFormComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/notifications` | `NotificationListComponent` | `DashboardLayout` | `AuthGuard` | Todos |
| `/users` | `UserListComponent` | `DashboardLayout` | `AuthGuard` | Admin |
| `/inventory-movements` | `InventoryMovementListComponent` | `DashboardLayout` | `AuthGuard` | Todos |

Las rutas se implementarán con **lazy loading** mediante `loadComponent` en `app.routes.ts`. El layout Ionic se estructura con `ion-router-outlet` dentro de cada layout (auth vs dashboard), y los componentes envuelven su contenido con `ion-page > ion-header + ion-content + ion-footer`.

### 4.1 Estructura de Layout Ionic

```html
<!-- auth-layout.component.html -->
<ion-content class="ion-padding">
  <ion-router-outlet></ion-router-outlet>
</ion-content>
```

```html
<!-- dashboard-layout.component.html -->
<ion-split-pane contentId="main-content">
  <ion-menu contentId="main-content">
    <ion-header>
      <ion-toolbar color="primary">
        <ion-title>AutoNex</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-list>
        <ion-menu-toggle auto-hide="false" *ngFor="let item of menuItems">
          <ion-item [routerLink]="item.path" routerDirection="root">
            <ion-icon [name]="item.icon" slot="start"></ion-icon>
            <ion-label>{{ item.label }}</ion-label>
          </ion-item>
        </ion-menu-toggle>
      </ion-list>
    </ion-content>
  </ion-menu>

  <ion-router-outlet id="main-content"></ion-router-outlet>
</ion-split-pane>
```

### 4.2 Patrón de Página Ionic

Cada feature component sigue la estructura:

```html
<ion-header>
  <ion-toolbar color="primary">
    <ion-buttons slot="start">
      <ion-menu-button></ion-menu-button>
      <ion-back-button></ion-back-button>
    </ion-buttons>
    <ion-title>Título de Página</ion-title>
    <ion-buttons slot="end">
      <ion-button (click)="onAction()">Acción</ion-button>
    </ion-buttons>
  </ion-toolbar>
</ion-header>

<ion-content class="ion-padding">
  <!-- Contenido específico del feature -->
</ion-content>
```

---

## 5. Flujo de Autenticación

```
Login ──> POST /api/auth/login ──> JWT ──> localStorage + AuthStateService
                                              │
                                              ▼
                                    AuthInterceptor añade
                                    Bearer token a cada request
                                              │
                                              ▼
                                    AuthGuard verifica token
                                    (existencia + expiración)
                                              │
                                              ▼
                                    RoleGuard verifica claim
                                    "role" contra ruta requerida
                                              │
                                              ▼
                                    ErrorInterceptor captura
                                    401 ──> limpia estado ──> redirect /auth/login
```

### 5.1 AuthStateService (Signals)

```typescript
@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly userSignal = signal<UserResponse | null>(null);
  private readonly tokenSignal = signal<string | null>(
    localStorage.getItem('token')
  );

  readonly user = this.userSignal.asReadonly();
  readonly token = this.tokenSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.tokenSignal() !== null);
  readonly role = computed(() => this.userSignal()?.role ?? null);

  setAuth(user: UserResponse, token: string): void {
    localStorage.setItem('token', token);
    this.userSignal.set(user);
    this.tokenSignal.set(token);
  }

  clearAuth(): void {
    localStorage.removeItem('token');
    this.userSignal.set(null);
    this.tokenSignal.set(null);
  }
}
```

### 5.2 AuthService

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private readonly api: ApiService,
    private readonly authState: AuthStateService,
  ) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/login', request);
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/register', request);
  }

  logout(): void {
    this.authState.clearAuth();
  }
}
```

---

## 6. Manejo de Estado con Signals

No se utilizará NgRx ni librerías externas de estado. El estado de la aplicación se gestionará mediante **Angular Signals**, aprovechando su integración nativa con el framework.

### 6.1 Patrón por Feature Service

Cada módulo de negocio contará con un servicio específico que expone Signals para:
- `data`: señal con la lista o entidad actual
- `loading`: señal booleana de carga
- `error`: señal con el mensaje de error
- `pagination`: señal con metadatos de paginación (para listas)

```typescript
@Injectable({ providedIn: 'root' })
export class ClientService {
  private readonly clientsSignal = signal<ClientResponse[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);
  private readonly paginationSignal = signal<PaginationMeta | null>(null);

  readonly clients = this.clientsSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly pagination = this.paginationSignal.asReadonly();

  constructor(private readonly api: ApiService) {}

  loadAll(params?: ClientFilterParams): Observable<void> {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    return this.api.getPaged<ClientResponse>('/clients', params).pipe(
      tap(response => {
        this.clientsSignal.set(response.items);
        this.paginationSignal.set({
          page: response.page,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
        });
        this.loadingSignal.set(false);
      }),
      catchError(err => {
        this.errorSignal.set(err.message);
        this.loadingSignal.set(false);
        return throwError(() => err);
      }),
    );
  }
}
```

---

## 7. ApiService — Servicio Base HTTP

Servicio genérico que encapsula `HttpClient` y centraliza el manejo del envelope `ApiResponse<T>`.

```typescript
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = 'http://localhost:5212/api';

  constructor(private readonly http: HttpClient) {}

  get<T>(path: string, params?: HttpParams): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${this.baseUrl}${path}`, { params })
      .pipe(map(res => res.data));
  }

  getPaged<T>(path: string, params?: HttpParams): Observable<PagedResponse<T>> {
    return this.http
      .get<ApiResponse<PagedResponse<T>>>(`${this.baseUrl}${path}`, { params })
      .pipe(map(res => res.data));
  }

  getById<T>(path: string, id: number): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${this.baseUrl}${path}/${id}`)
      .pipe(map(res => res.data));
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(`${this.baseUrl}${path}`, body)
      .pipe(map(res => res.data));
  }

  put<T>(path: string, id: number, body: unknown): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(`${this.baseUrl}${path}/${id}`, body)
      .pipe(map(res => res.data));
  }

  patch<T>(path: string, id: number, body: unknown): Observable<T> {
    return this.http
      .patch<ApiResponse<T>>(`${this.baseUrl}${path}/${id}`, body)
      .pipe(map(res => res.data));
  }

  delete<T>(path: string, id: number): Observable<T> {
    return this.http
      .delete<ApiResponse<T>>(`${this.baseUrl}${path}/${id}`)
      .pipe(map(res => res.data));
  }
}
```

---

## 8. Interceptors

### 8.1 AuthInterceptor

```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private readonly authState: AuthStateService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    const token = this.authState.token();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` },
      });
    }
    return next(req);
  }
}
```

### 8.2 ErrorInterceptor

```typescript
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private readonly authState: AuthStateService,
    private readonly router: Router,
  ) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    return next(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.authState.clearAuth();
          this.router.navigate(['/auth/login']);
        }
        // 400: validation errors propagated as-is
        // 403: router.navigate(['/forbidden'])
        // 500: generic error message
        return throwError(() => this.normalizeError(error));
      }),
    );
  }

  private normalizeError(error: HttpErrorResponse): AppError {
    if (error.status === 400) {
      return { message: error.error?.message, validationErrors: error.error?.errors };
    }
    if (error.status === 403) {
      return { message: 'No tienes permisos para esta acción' };
    }
    return { message: 'Error inesperado. Intenta de nuevo.' };
  }
}
```

---

## 9. Guards

### 9.1 AuthGuard

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  if (authState.isAuthenticated()) return true;

  return router.parseUrl('/auth/login');
};
```

### 9.2 RoleGuard

```typescript
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authState = inject(AuthStateService);
    const router = inject(Router);
    const role = authState.role();

    if (role && allowedRoles.includes(role)) return true;

    return router.parseUrl('/dashboard');
  };
};
```

---

## 10. Diseño de Componentes (Ionic)

### 10.1 Patrón de Lista (`*ListComponent`)

Todo listado usa componentes nativos de Ionic:

```html
<ion-header>
  <ion-toolbar color="primary">
    <ion-buttons slot="start">
      <ion-menu-button></ion-menu-button>
    </ion-buttons>
    <ion-title>Clientes</ion-title>
    <ion-buttons slot="end">
      <ion-button routerLink="/clients/new" *ngIf="canCreate()">
        <ion-icon name="add" slot="icon-only"></ion-icon>
      </ion-button>
    </ion-buttons>
  </ion-toolbar>
</ion-header>

<ion-content>
  <ion-searchbar [(ngModel)]="searchTerm"
                 (ionInput)="onSearch($event)"
                 placeholder="Buscar por nombre o teléfono..."
                 debounce="300">
  </ion-searchbar>

  @if (loading()) {
    <ion-list>
      @for (_ of [1,2,3,4,5]; track $index) {
        <ion-item>
          <ion-label>
            <ion-skeleton-text animated style="width: 60%"></ion-skeleton-text>
          </ion-label>
        </ion-item>
      }
    </ion-list>
  } @else if (clients().length === 0) {
    <div class="flex flex-col items-center justify-center py-16">
      <ion-icon name="people-outline" class="text-6xl text-gray-300"></ion-icon>
      <p class="text-gray-500 mt-4">No se encontraron clientes</p>
    </div>
  } @else {
    <ion-list>
      @for (client of clients(); track client.id) {
        <ion-item [routerLink]="['/clients', client.id]" detail>
          <ion-label>
            <h2>{{ client.fullName }}</h2>
            <p>{{ client.phone }}@if (client.email) { — {{ client.email }} }</p>
          </ion-label>
          <ion-note slot="end">{{ client.createdAt | dateFormat }}</ion-note>
        </ion-item>
      }
    </ion-list>

    <ion-infinite-scroll (ionInfinite)="loadMore($event)">
      <ion-infinite-scroll-content></ion-infinite-scroll-content>
    </ion-infinite-scroll>
  }
</ion-content>
```

Características:
- `ion-searchbar` con debounce de 300ms para filtro
- `ion-list` + `ion-item` para renderizado nativo
- Paginación con `ion-infinite-scroll` (scroll infinito) o `ion-pagination` para control manual
- `ion-skeleton-text` para estado de carga
- Estado vacío con ícono Ionic y mensaje
- Botón "+" en toolbar según permisos del rol
- Pull-to-refresh opcional con `ion-refresher`

### 10.2 Patrón de Formulario (`*FormComponent`)

```html
<ion-header>
  <ion-toolbar color="primary">
    <ion-buttons slot="start">
      <ion-back-button></ion-back-button>
    </ion-buttons>
    <ion-title>{{ isEditMode() ? 'Editar' : 'Nuevo' }} Cliente</ion-title>
    <ion-buttons slot="end">
      <ion-button (click)="onCancel()" color="light">Cancelar</ion-button>
    </ion-buttons>
  </ion-toolbar>
</ion-header>

<ion-content class="ion-padding">
  <form [formGroup]="form" (ngSubmit)="onSubmit()">
    <ion-list>
      <ion-item>
        <ion-label position="floating">Nombre Completo</ion-label>
        <ion-input formControlName="fullName" type="text"></ion-input>
      </ion-item>
      @if (form.get('fullName')?.invalid && form.get('fullName')?.touched) {
        <ion-note color="danger" class="px-4">El nombre es requerido</ion-note>
      }

      <ion-item>
        <ion-label position="floating">Teléfono</ion-label>
        <ion-input formControlName="phone" type="tel"></ion-input>
      </ion-item>

      <ion-item>
        <ion-label position="floating">Email</ion-label>
        <ion-input formControlName="email" type="email"></ion-input>
      </ion-item>

      <ion-item>
        <ion-label position="floating">Dirección</ion-label>
        <ion-textarea formControlName="address" rows="3"></ion-textarea>
      </ion-item>
    </ion-list>

    <ion-button type="submit" expand="block" class="mt-6"
                [disabled]="form.invalid || saving()">
      {{ saving() ? 'Guardando...' : 'Guardar' }}
    </ion-button>
  </form>
</ion-content>
```

Características:
- `ion-label position="floating"` para etiquetas animadas
- `ion-input`, `ion-textarea`, `ion-select`, `ion-datetime` para campos
- `ion-note color="danger"` para errores de validación
- Reactive forms con validadores sincrónicos
- Modo creación/edición según presencia de `id` en ruta
- Botón submit con `expand="block"`
- Loading state en botón durante submit

### 10.3 Patrón Dashboard

```html
<ion-header>
  <ion-toolbar color="primary">
    <ion-buttons slot="start">
      <ion-menu-button></ion-menu-button>
    </ion-buttons>
    <ion-title>Dashboard</ion-title>
  </ion-toolbar>
</ion-header>

<ion-content class="ion-padding">
  <!-- Summary Cards -->
  <ion-grid>
    <ion-row>
      <ion-col size="6" size-md="3" *ngFor="let card of summaryCards">
        <ion-card class="h-full" [routerLink]="card.link">
          <ion-card-content class="text-center">
            <ion-icon [name]="card.icon" class="text-3xl" color="primary"></ion-icon>
            <ion-label class="block text-2xl font-bold mt-2">{{ card.value }}</ion-label>
            <ion-label class="text-sm text-gray-500">{{ card.label }}</ion-label>
          </ion-card-content>
        </ion-card>
      </ion-col>
    </ion-row>
  </ion-grid>

  <!-- Recent Orders -->
  <ion-card>
    <ion-card-header>
      <ion-card-title>Órdenes Recientes</ion-card-title>
      <ion-card-subtitle>Últimas 5 órdenes de servicio</ion-card-subtitle>
    </ion-card-header>
    <ion-card-content>
      <ion-list>
        <ion-item *ngFor="let order of recentOrders()" [routerLink]="['/service-orders', order.id]" detail>
          <ion-label>
            <h2>{{ order.vehicleInfo }}</h2>
            <p>{{ order.clientName }} — {{ order.status | enumLabel }}</p>
          </ion-label>
          <ion-badge [color]="statusColor(order.status)">{{ order.status }}</ion-badge>
        </ion-item>
      </ion-list>
    </ion-card-content>
  </ion-card>
</ion-content>
```

---

## 11. Convenciones de Código

| Aspecto | Convención |
|---|---|---|
| Nomenclatura TypeScript | `camelCase` para variables, propiedades y métodos; `PascalCase` para clases, interfaces y tipos |
| Nomenclatura archivos | `kebab-case` para nombres de archivo (ej. `client-list.component.ts`) |
| Estructura imports | Angular core → librerías externas → servicios → modelos → utilerías (separados por línea en blanco) |
| Standalone components | Todos los componentes serán standalone, sin NgModules |
| Detección de cambios | `ChangeDetectionStrategy.OnPush` en todos los componentes |
| Inyección de dependencias | `inject()` function (no constructor injection) |
| Formateo | Prettier con printWidth 100, single quotes, trailingCommas all |
| Estilos | Tailwind + Ionic CSS Custom Properties; usar variables CSS de Ionic para consistencia (`--ion-color-primary`, etc.) |
| Tipado | `strict: true` en tsconfig; evitar `any` |
| Modelos | Interfaces TypeScript con nombres exactos al backend: `ClientResponse`, `CreateClientRequest`, etc. |
| Enums | TypeScript enums con mismos valores que backend (`Admin = 'Admin'`) |
| Observables | Usar `takeUntilDestroyed` o `DestroyRef` para evitar suscripciones huérfanas |
| Paginación | Server-side; usar `ion-infinite-scroll` para carga progresiva o `ion-pagination` para control manual |
| Navegación | `ion-router-outlet` + `[routerLink]` + `routerDirection` ("root", "forward", "back") |
| Componentes Ionic | Preferir `ion-*` components sobre HTML nativo para listas, tarjetas, formularios, modales, toasts |
| Alertas/Confirmaciones | Usar `IonAlertController` e `IonToastController` en lugar de `window.confirm`/`window.alert` |
| Íconos | Ionicons (`ion-icon`) — conjunto cerrado, sin librería adicional |

---

## 12. Modelos Compartidos (TypeScript Interfaces)

### 12.1 Enums

```typescript
export enum UserRole {
  Admin = 'Admin',
  Mechanic = 'Mechanic',
  Receptionist = 'Receptionist',
}

export enum ConsumableCategory {
  Oil = 'Oil',
  SparkPlug = 'SparkPlug',
  Coolant = 'Coolant',
  Grease = 'Grease',
  BrakeFluid = 'BrakeFluid',
  Other = 'Other',
}

export enum ToolCategory {
  Jack = 'Jack',
  Wrench = 'Wrench',
  Ratchet = 'Ratchet',
  Screwdriver = 'Screwdriver',
  Hammer = 'Hammer',
  Other = 'Other',
}

export enum ToolStatus {
  Available = 'Available',
  Damaged = 'Damaged',
  Lost = 'Lost',
}

export enum ServiceOrderStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

export enum FinancialRecordType {
  Income = 'Income',
  Expense = 'Expense',
}

export enum FinancialCategory {
  Services = 'Services',
  Suppliers = 'Suppliers',
  Rent = 'Rent',
  Payroll = 'Payroll',
  Utilities = 'Utilities',
  Other = 'Other',
}

export enum NotificationType {
  WhatsApp = 'WhatsApp',
  Sms = 'Sms',
  Email = 'Email',
}

export enum NotificationStatus {
  Pending = 'Pending',
  Sent = 'Sent',
  Failed = 'Failed',
}

export enum MovementType {
  In = 'In',
  Out = 'Out',
}
```

### 12.2 Envelope y Paginación

```typescript
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors?: Record<string, string[]>;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
```

### 12.3 Auth

```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: UserRole;
  phone?: string;
}

export interface AuthResponse {
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
  token: string;
}
```

### 12.4 Clientes

```typescript
export interface ClientResponse {
  id: number;
  fullName: string;
  phone: string;
  email?: string;
  address?: string;
  createdAt: string;
  vehicles?: VehicleBriefResponse[];
}

export interface CreateClientRequest {
  fullName: string;
  phone: string;
  email?: string;
  address?: string;
}

export type UpdateClientRequest = CreateClientRequest;
```

### 12.5 Vehículos

```typescript
export interface VehicleBriefResponse {
  id: number;
  brand: string;
  model: string;
  year: number;
  licensePlate: string;
}

export interface VehicleResponse {
  id: number;
  clientId: number;
  clientName: string;
  brand: string;
  model: string;
  year: number;
  licensePlate: string;
  vin?: string;
  createdAt: string;
}

export interface CreateVehicleRequest {
  clientId: number;
  brand: string;
  model: string;
  year: number;
  licensePlate: string;
  vin?: string;
}

export interface UpdateVehicleRequest {
  brand: string;
  model: string;
  year: number;
  licensePlate: string;
  vin?: string;
}
```

### 12.6 Órdenes de Servicio

```typescript
export interface ServiceOrderResponse {
  id: number;
  vehicleId: number;
  vehicleInfo: string;
  clientId: number;
  clientName: string;
  userId: number;
  userName: string;
  currentKm: number;
  date: string;
  status: ServiceOrderStatus;
  totalAmount: number;
  notes?: string;
  createdAt: string;
  items: ServiceOrderItemResponse[];
}

export interface ServiceOrderItemResponse {
  id: number;
  serviceId?: number;
  serviceName?: string;
  consumableId?: number;
  consumableName?: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateServiceOrderRequest {
  vehicleId: number;
  clientId: number;
  currentKm: number;
  notes?: string;
  items: CreateServiceOrderItemRequest[];
}

export interface CreateServiceOrderItemRequest {
  serviceId?: number;
  consumableId?: number;
  quantity: number;
  unitPrice: number;
}

export interface UpdateServiceOrderStatusRequest {
  status: ServiceOrderStatus;
}
```

### 12.7 Modelos Restantes

Los modelos para `Supplier`, `Consumable`, `Tool`, `Service`, `MileageAlert`, `FinancialRecord`, `FinancialSummary`, `CategorySummary`, `Notification` e `InventoryMovement` siguen la misma estructura que sus correspondientes DTOs C# del backend. Ver `docs/API.md` para la referencia completa de campos.

---

## 13. Plan de Implementación por Etapas

### Etapa 1 — Fundación (Sprint 1, Semana 1)

| Actividad | Entregable |
|---|---|
| Scaffolding con Ionic CLI | `ionic start frontend blank --type=angular-standalone` |
| Configuración de Tailwind CSS | `tailwind.config.js`, estilos base + integración con Ionic |
| Configuración de Capacitor | `capacitor.config.ts`, `ionic build` + `npx cap copy` |
| Configuración de ESLint + Prettier | Reglas de calidad |
| Tema Ionic personalizado | `src/theme/variables.scss` con paleta de colores del taller |
| Creación de `ApiService` | Servicio base HTTP |
| Creación de modelos (`core/models/`) | Todas las interfaces TypeScript |
| Creación de interceptors | Auth + Error |
| Creación de guards | AuthGuard + RoleGuard |
| Creación de `AuthStateService` | Signals de autenticación |
| Módulo Auth (Login + Register) | Componentes Ionic con ion-input, ion-button, ion-card |
| Layouts (AuthLayout + DashboardLayout) | Layouts con ion-router-outlet, ion-split-pane, ion-menu |
| Dashboard | Cards de resumen con ion-card, ion-grid, datos mock |
| Mapa de rutas completo | `app.routes.ts` con lazy loading |

### Etapa 2 — Clientes y Vehículos (Sprint 1, Semana 2)

| Actividad | Entregable |
|---|---|
| `ClientService` + Signals | Servicio con loadAll, create, update, delete |
| ClientListComponent | Tabla con búsqueda y paginación |
| ClientFormComponent | Formulario creación/edición |
| ClientDetailComponent | Vista detalle con vehículos asociados |
| `VehicleService` + Signals | Servicio completo |
| VehicleListComponent | Tabla con búsqueda |
| VehicleFormComponent | Formulario con selector de cliente |
| VehicleDetailComponent | Vista detalle |

### Etapa 3 — Catálogos (Sprint 2, Semana 3)

| Actividad | Entregable |
|---|---|
| SupplierListComponent | CRUD completo (Admin en create/edit) |
| ConsumableListComponent | Tabla con filtro por categoría |
| LowStockComponent | Alerta visual de stock mínimo |
| ConsumableFormComponent | Formulario con selector de proveedor |
| ToolListComponent | Tabla con filtros por categoría/estado |
| ToolFormComponent | Formulario con selector de estado |

### Etapa 4 — Catálogo de Servicios (Sprint 2, Semana 3)

| Actividad | Entregable |
|---|---|
| `ServiceService` + Signals | CRUD servicios |
| ServiceListComponent | Tabla con búsqueda |
| ServiceFormComponent | Formulario con precio default, intervalos de km y meses |

### Etapa 5 — Órdenes de Servicio (Sprint 3, Semana 4)

| Actividad | Entregable |
|---|---|
| `ServiceOrderService` + Signals | CRUD completo |
| ServiceOrderListComponent | Tabla con filtros por fecha/cliente/estado |
| ServiceOrderFormComponent | Formulario con grid de items dinámico, selectores de servicio/consumible, cálculo automático de total |
| ServiceOrderDetailComponent | Vista detalle con items, acciones de cambio de estado |
| Status management | PATCH de estado con confirmación |

### Etapa 6 — Alertas y Notificaciones (Sprint 3, Semana 4)

| Actividad | Entregable |
|---|---|
| `MileageAlertService` + Signals | CRUD + filtro due |
| MileageAlertListComponent | Tabla con indicador visual de vencimiento |
| MileageAlertFormComponent | Configuración de km estimados |
| `NotificationService` + Signals | Listado + envío manual |
| NotificationListComponent | Historial con filtros |

### Etapa 7 — Finanzas (Sprint 4, Semana 5)

| Actividad | Entregable |
|---|---|
| `FinancialRecordService` + Signals | CRUD + summary + byCategory |
| FinancialRecordListComponent | Tabla con filtros |
| FinancialRecordFormComponent | Formulario con selectores de tipo/categoría |
| FinancialSummaryComponent | Gráficos ngx-charts: ingresos vs gastos, categorías |

### Etapa 8 — Usuarios y Movimientos (Sprint 4, Semana 5)

| Actividad | Entregable |
|---|---|
| `UserService` + Signals | Listado + actualización |
| UserListComponent | Tabla solo lectura con roles (Admin) |
| `InventoryMovementService` + Signals | Listado con filtros |
| InventoryMovementListComponent | Tabla histórico de movimientos |

### Etapa 9 — Pulido y Responsive (Sprint 4, Semana 6)

| Actividad | Entregable |
|---|---|
| Estados de carga | `ion-skeleton-text` en todas las listas |
| Estados vacíos | Mensajes informativos con ion-icon cuando no hay datos |
| Estados de error | `ion-toast` + `ion-alert` para notificaciones de error |
| Pull-to-refresh | `ion-refresher` en todas las listas |
| Responsive design | `ion-split-pane` para sidebar adaptable, modales responsivos |
| Dark mode (opcional) | Soporte con CSS variables de Ionic + `prefers-color-scheme` |
| Pruebas manuales | Recorrido completo por todos los flujos |
| PWA (opcional) | `@angular/pwa` + Capacitor para instalación en dispositivo |
| Optimización | Lazy loading verificado, bundle size, Ionic lazy loading de íconos |

---

## 14. API Reference

Para la especificación completa de cada endpoint, incluyendo ejemplos de request/response y códigos HTTP, consultar:

- [`docs/API.md`](./API.md) — Referencia completa de la API REST
- [`docs/ARCHITECTURE.md`](./ARCHITECTURE.md) — Modelo de datos, entidades y relaciones
- [`docs/AUDITORIA_DOCS_VS_CODIGO.md`](./AUDITORIA_DOCS_VS_CODIGO.md) — Discrepancias conocidas entre documentación y código real

---

## 15. Estimación

| Recurso | Dedicación |
|---|---|
| 1 Desarrollador Frontend Senior | 6 semanas (MVP completo) |
| 1 Desarrollador Backend (apoyo) | 2 horas/semana para ajustes de API |

Total estimado: **240 horas-hombre** para MVP funcional con todos los módulos operativos.
