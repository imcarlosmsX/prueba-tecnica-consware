# Contrato de Dominio (extraído del enunciado)

> Fuente: `Prueba-Tecnica-Consware (1) 1.md`, secciones 2 y 3.
> Todo lo de aquí está transcrito del enunciado. Lo que no está aquí, no se construye.

## Actores

- Actor que CREA (app móvil / Flutter): **Empleado**
- Actor que DECIDE (panel web / Angular): **Aprobador** (jefe de área)

## Entidad principal

| | |
| --- | --- |
| Nombre en el enunciado | Solicitud de reembolso |
| Nombre en código | `ReimbursementRequest` |
| Tabla | `ReimbursementRequests` |

Entidad de apoyo: `Employee` (`Employees`). El enunciado permite identificar al empleado
"de forma sencilla (un nombre fijo o seleccionable al iniciar)", pero **exige filtrar por
empleado**, así que necesita ser una fila real con id estable, no un string suelto.

## Campos y validaciones

| Campo (inglés) | Tipo | Obligatorio | Validación literal del enunciado |
| --- | --- | --- | --- |
| `Id` | `Guid` | generado | — |
| `EmployeeId` | `Guid` | sí | El empleado debe existir. |
| `Amount` | `decimal(18,2)` | sí | "monto positivo" |
| `Category` | `ExpenseCategory` (enum) | sí | "categoría [...] presente" |
| `Description` | `string(500)` | sí | "descripción presente" |
| `Status` | `ReimbursementStatus` (enum) | sí | "Cada solicitud nace en estado Pendiente" |
| `RejectionReason` | `string(500)?` | solo al rechazar | "Ninguna solicitud puede rechazarse sin un motivo escrito." |
| `CreatedAtUtc` | `DateTime` | sí | — |
| `DecidedAtUtc` | `DateTime?` | al decidirse | — |

## Máquina de estados

- Estados: `Pending` (inicial) → `Approved` | `Rejected` (terminales)
- ¿Los estados terminales son inmutables? **SÍ** — cita textual: *"Una solicitud ya Aprobada
  o Rechazada no cambia de estado."*

## Reglas de negocio (transcritas LITERALMENTE, numeradas)

```
R1. Cada solicitud nace en estado Pendiente y termina Aprobada o Rechazada.
R2. Las solicitudes de $500.000 COP o menos se aprueban automáticamente al crearse.
R3. Las solicitudes de más de $500.000 COP quedan Pendientes y requieren decisión del aprobador.
R4. Ninguna solicitud puede rechazarse sin un motivo escrito.
R5. Una solicitud ya Aprobada o Rechazada no cambia de estado.
R6. Valida los datos (monto positivo, categoría y descripción presentes).
R7. Aprobar: solo sobre solicitudes Pendientes.
R8. Rechazar: solo sobre solicitudes Pendientes y con motivo obligatorio.
```

## Regla automática / umbral

- Condición: `Amount <= 500000` (COP)
- Efecto: la solicitud se crea directamente en `Approved`, con `DecidedAtUtc` sellado.
- Constante: `ReimbursementPolicy.AutomaticApprovalThresholdCop`

## Campo obligatorio en el camino negativo

- `RejectionReason`, obligatorio y no vacío al rechazar (R4).

## Operaciones pedidas

Base: `http://localhost:5080/api/v1`

| Operación | Verbo + ruta | Éxito | Errores |
| --- | --- | --- | --- |
| Crear solicitud | `POST /reimbursement-requests` | `201` + `Location` | `400` validación |
| Listar solicitudes | `GET /reimbursement-requests?status=&employeeId=` | `200` (array) | `400` filtro inválido |
| Consultar por id | `GET /reimbursement-requests/{id}` | `200` | `404` no existe |
| Aprobar | `POST /reimbursement-requests/{id}/approve` | `200` | `404`, `409` no está Pendiente |
| Rechazar | `POST /reimbursement-requests/{id}/reject` | `200` | `404`, `400` motivo vacío, `409` no está Pendiente |
| Listar empleados | `GET /employees` | `200` | — |

`GET /employees` no lo pide el enunciado como operación, pero sí pide que el empleado sea
"seleccionable al iniciar" en Flutter y filtrable en la API. Es el mínimo para que eso funcione
sin hardcodear ids en el móvil.

## Filtros de listado exigidos

- `status` (Pending / Approved / Rejected)
- `employeeId`

## Ambigüedades del enunciado y su interpretación

- ⚠️ **AMBIGUO: ¿el umbral es inclusivo?** El enunciado dice "$500.000 COP **o menos**".
  Interpretación: **inclusivo (`<=`)**. Exactamente 500.000 se aprueba automáticamente.
- ⚠️ **AMBIGUO: ¿la categoría es texto libre o catálogo?** El enunciado da ejemplos cerrados
  (almuerzo, taxi, papelería). Interpretación: **enum `ExpenseCategory`**, porque un texto
  libre haría inútil cualquier reporte y no permite validar "categoría presente" de verdad.
- ⚠️ **AMBIGUO: ¿longitud mínima del motivo de rechazo?** El enunciado solo dice "motivo
  escrito". Interpretación: **no vacío ni solo espacios**, máximo 500 caracteres. No se
  inventa un mínimo de N caracteres que el enunciado no pide.
- ⚠️ **AMBIGUO: ¿moneda?** Siempre COP. Se expone como constante en las respuestas, no como
  columna: no hay multi-moneda en el enunciado.

## Lo que el enunciado NO pide (NO construir)

- Autenticación / autorización ("No es necesario implementar autenticación")
- Editar o borrar solicitudes
- Notificaciones, correos, reportes, exportaciones
- Adjuntar comprobantes o imágenes
- Paginación del listado
- Jerarquía de aprobadores, montos por área, presupuestos
- Historial o auditoría de cambios
