# Sistema de Reembolso de Gastos

Prueba técnica fullstack · ASP.NET Core · Flutter · Angular

Un empleado registra solicitudes de reembolso desde su celular; un aprobador las decide desde un
panel web. Las solicitudes de **$500.000 COP o menos se aprueban automáticamente al crearse**; las
mayores quedan pendientes de decisión.

---

## 1. Qué incluye esta entrega

| Parte | Estado | Detalle |
| --- | --- | --- |
| **Parte 1 · API ASP.NET** | ✅ Completo | Clean Architecture, EF Core + SQLite con migración real, 6 endpoints, errores RFC 7807, Swagger |
| **Parte 2 · App Flutter** | ✅ Completo | Selección de empleado, formulario validado, listado con estados y motivo de rechazo, manejo de red resiliente |
| **Parte 3 · Panel Angular** | ✅ Completo | Tabla con filtro por estado (Pendientes por defecto), aprobar/rechazar, motivo obligatorio con Reactive Forms |
| Pruebas unitarias | ✅ 67 tests | 48 de dominio (sin mocks) + 19 de casos de uso (con Moq) |

La app Flutter se verificó **en el emulador de Android** (Pixel, API 36) y también corre en Chrome.

---

## 2. Arquitectura

```
netapi/                        appflutter/                    appangular/
  Api ──────────┐                Widget                         Componente contenedor
  Infrastructure├─► Application    └─► Provider                    └─► Servicio
                └────► Domain           └─► Service                     └─► HttpClient
                        ▲                    └─► ApiClient                   + interceptor
                        │
        Las dependencias apuntan SIEMPRE hacia adentro.
        Domain no tiene ni una referencia externa: ni EF Core, ni ASP.NET.
```

**La regla de negocio vive en un solo lugar**: `ReimbursementRequest` (la entidad) y
`ReimbursementPolicy` (el umbral). Ni los controllers, ni los widgets de Flutter, ni los
componentes de Angular mencionan el número 500.000.

---

## 3. Cómo ejecutarlo

### 3.1 Requisitos previos

| Herramienta | Versión usada |
| --- | --- |
| .NET SDK | 10.0.401 |
| Flutter | 3.47.4 (Dart 3.11) |
| Node.js | 22 o superior (probado con v24.19.0) |
| Android SDK | 36 (opcional: Flutter también corre en Chrome) |

No hace falta instalar ningún motor de base de datos: se usa SQLite y el archivo se crea solo.

### 3.2 Base de datos

**No hay que hacer nada.** Al arrancar, la API aplica las migraciones de EF Core y siembra tres
empleados de ejemplo. El archivo `expense-reimbursement.db` se crea junto al proyecto de la API.

Para empezar de cero en cualquier momento, basta con borrarlo:

```bash
rm netapi/src/ExpenseReimbursement.Api/expense-reimbursement.db
```

Si prefieres aplicar la migración a mano antes de arrancar:

```bash
cd netapi
dotnet ef database update --project src/ExpenseReimbursement.Infrastructure --startup-project src/ExpenseReimbursement.Api
```

### 3.3 API (ejecutar primero)

```bash
cd netapi/src/ExpenseReimbursement.Api
dotnet run
```

- API: **http://localhost:5080/api/v1**
- Swagger UI: **http://localhost:5080/swagger**

Para correr las pruebas:

```bash
cd netapi
dotnet test
```

### 3.4 App Flutter

Con la API ya corriendo:

```bash
cd appflutter
flutter pub get
flutter run
```

> **Nota sobre la URL en Android.** Dentro del emulador, `localhost` es el propio dispositivo
> virtual, no tu PC. Por eso la app usa `http://10.0.2.2:5080` en Android y `http://localhost:5080`
> en web y escritorio. Está resuelto en `lib/core/app_config.dart`; no hay que cambiar nada.

Para elegir dispositivo explícitamente:

```bash
flutter run -d emulator-5554     # emulador de Android
flutter run -d chrome            # navegador, si no hay emulador a mano
```

### 3.5 Panel Angular

Con la API ya corriendo:

```bash
cd appangular
npm install
npx ng serve
```

Panel: **http://localhost:4200**

---

## 4. Decisiones técnicas y por qué

| Decisión | Alternativa descartada | Razón |
| --- | --- | --- |
| **La regla de negocio en la entidad de dominio**, con `private set` y transiciones sólo vía `Approve()` / `Reject(reason)` | Un POCO con setters públicos y la lógica en un servicio | Hace **imposible por construcción** dejar una solicitud en un estado inválido. La inmutabilidad de una decisión ya tomada no depende de que alguien recuerde validarla |
| **El umbral como constante en `ReimbursementPolicy`** | Comparar contra `500_000` dentro de `Create()` | Es el único lugar del proyecto donde existe ese número. Cambiar la política toca **un archivo, una línea** |
| **`409 Conflict` en transiciones inválidas** | `400 Bad Request` | La petición está bien formada; lo que falla es que choca con el estado actual del recurso. Un 400 diría "corrige tu petición" cuando no hay nada que corregir |
| **SQLite** | SQL Server LocalDB | El enunciado autoriza cualquier motor SQL. SQLite hace que el proyecto corra sin instalar nada. Migrar es cambiar `UseSqlite` por `UseSqlServer` en `DependencyInjection.cs` |
| **Handlers escritos a mano, un archivo por caso de uso** | MediatR · un `ReimbursementService` con 6 métodos | MediatR añade indirección sin beneficio en este tamaño. Un servicio con 6 métodos es el "controller gordo" disfrazado |
| **Mapeo manual con `ToResponse()`** | AutoMapper | Diez líneas que el compilador verifica, en vez de convenciones que fallan en silencio al renombrar una propiedad |
| **Enums persistidos como texto** (`"Pending"`) | Persistir el entero | La tabla se lee sin diccionario de códigos y, sobre todo, **reordenar el enum no reinterpreta las filas existentes** |
| **`POST /{id}/approve`** | `PUT /{id}` | Un PUT invitaría al cliente a enviar el `Status` y decidir él el estado. El POST deja claro que quien decide es el servidor |
| **`decimal` con precisión (18,2)** | `double` | Los binarios flotantes no representan valores decimales exactos; en dinero eso son descuadres de centavos |
| **Provider en Flutter** | Bloc · Riverpod | Separa UI de lógica con la mínima ceremonia. Bloc es excelente pero su boilerplate no se amortiza en tres pantallas |
| **Signals + componentes standalone en Angular** | NgModules · NgRx | Menos ceremonia y estado explícito. NgRx sería sobreingeniería para una pantalla |
| **Reactive Forms para el motivo de rechazo** | Template-Driven (`ngModel`) | La validación es un objeto consultable y comprobable, no atributos repartidos por el HTML |
| **Un único middleware / interceptor de errores** | `try/catch` por endpoint o por componente | El mapeo error → mensaje existe en un solo lugar y un endpoint nuevo lo hereda sin escribir nada |

### Interpretaciones de puntos que el enunciado no define

Están documentadas en `DOMAIN-CONTRACT.md`. Las relevantes:

- **El umbral es inclusivo.** "$500.000 COP o menos" → exactamente 500.000 se aprueba
  automáticamente. Hay un test dedicado a esa frontera.
- **La categoría es un enum**, no texto libre: el enunciado da ejemplos cerrados y un texto libre
  haría imposible validar que la categoría esté "presente" de forma significativa.
- **El motivo de rechazo** solo exige no estar vacío ni ser espacios en blanco. No se inventó un
  mínimo de caracteres que el enunciado no pide.
- **`GET /employees`** no lo pide el enunciado, pero sí exige filtrar por empleado y que el
  empleado sea "seleccionable al iniciar" en la app. Es el mínimo para no hardcodear ids.

---

## 5. Qué dejé por fuera por tiempo y cómo lo haría

- **Concurrencia entre aprobadores.** Si dos aprobadores deciden la misma solicitud a la vez, hoy
  gana el último y la segunda operación no falla como debería. Lo resolvería con concurrencia
  optimista de EF Core: una columna `rowversion` en la tabla y capturar
  `DbUpdateConcurrencyException` en el middleware para devolver un 409.
- **Tests de integración de la API.** Los códigos HTTP se verificaron manualmente con `curl`
  (creación en ambos lados del umbral, los cuatro caminos de error, persistencia tras reinicio).
  Los automatizaría con `WebApplicationFactory`, que levanta la API en memoria contra una base
  SQLite temporal y permite afirmar sobre el 409 de punta a punta.
- **Paginación del listado.** Con pocos cientos de solicitudes no se nota, pero no escala. Añadiría
  `page` y `pageSize` al repositorio y devolvería el total en la respuesta.
- **Autenticación y autorización.** El enunciado dice explícitamente que no es necesaria. Hoy
  cualquiera puede llamar a `/approve`. Con más tiempo: JWT y un rol de aprobador exigido en los
  endpoints de decisión.
- **Tests de widget en Flutter y de componente en Angular.** Prioricé los tests de la regla de
  negocio, que es lo que el enunciado evalúa. Añadiría un test de widget para el formulario y uno
  del `RejectDialogComponent` verificando que el botón no se habilita sin motivo.
- **La URL de la API está fija** en `reimbursement-api.service.ts` y `app_config.dart`. Debería
  venir de `environment.ts` y de `--dart-define` para poder apuntar a otro entorno sin recompilar.
- **`usesCleartextTraffic="true"`** en Android permite HTTP plano. Es aceptable en desarrollo
  local; en producción la API iría por HTTPS y esa bandera se quitaría.
- **CORS abierto** (`AllowAnyOrigin`) para no perder tiempo con orígenes en desarrollo. En
  producción se restringiría al dominio del panel.

---

## 6. Uso de IA

Usé **Claude (Claude Code)** como copiloto durante toda la sesión. Con detalle:

**En qué me apoyé en la IA**

- Generación del andamiaje repetitivo: estructura de los cuatro proyectos .NET, configuraciones de
  EF Core, widgets de Flutter y componentes de Angular a partir de decisiones que yo definí.
- Redacción de los tests una vez que yo determiné **qué casos había que cubrir** (la frontera del
  umbral, las cuatro combinaciones de re-decidir, el motivo en blanco).
- Traducción de mensajes de error de red a textos claros para el usuario final.
- Revisión de este README y del documento de decisiones.

**Qué decidí y validé yo**

- **La interpretación del enunciado**: el umbral inclusivo, la categoría como enum, el alcance de
  lo que NO se construye. Está en `DOMAIN-CONTRACT.md`, escrito antes de la primera línea de código.
- **Dónde vive la regla de negocio** y por qué la entidad tiene setters privados: es la decisión
  estructural de la que dependen las tres partes.
- **La semántica de los códigos HTTP**, en particular usar 409 y no 400 en transiciones inválidas,
  y distinguir el 404 de la URL del 400 de un id inválido dentro del cuerpo.
- **Qué NO usar**: descarté MediatR, AutoMapper, FluentValidation, Bloc y NgRx conscientemente,
  porque en un proyecto de este tamaño añaden indirección que hay que justificar sin beneficio real.
- **La verificación**. Ejecuté la API y probé cada endpoint con `curl`, corrí la app en el emulador
  de Android con la API encendida y apagada, y probé el panel con la API caída. Un caso concreto:
  `CreatedAtAction(nameof(GetByIdAsync))` compilaba pero fallaba en ejecución con 500, porque MVC
  recorta el sufijo `Async` de los nombres de acción; se detectó **ejecutando**, no leyendo.
- **La prueba de mutación**: cambié el `<=` por `<` en la política para comprobar que los tests
  fallaban donde debían. Fallaron exactamente los dos de la frontera.

**Herramienta**: Claude Code (Opus 5). Sin Copilot ni otras herramientas en esta sesión.
