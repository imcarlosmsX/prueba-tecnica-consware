import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

// DECISIÓN DE DISEÑO: un único interceptor traduce cualquier error HTTP a un Error con mensaje
// en español, y ningún componente inspecciona códigos de estado.
// POR QUÉ: es el espejo del middleware del backend. Sin él, cada componente tendría que saber
// qué significa un 409 y el texto se duplicaría en cada acción de la tabla.
// CONSECUENCIA: agregar una pantalla nueva hereda el manejo de errores sin escribir nada.
export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => throwError(() => new Error(toMessage(error)))),
  );

function toMessage(error: HttpErrorResponse): string {
  // status 0 significa que la petición ni siquiera salió: servidor caído, DNS o CORS.
  if (error.status === 0) {
    return 'No pudimos conectarnos al servidor. Verifica que la API esté corriendo.';
  }

  const detail = readProblemDetail(error);

  switch (error.status) {
    case 400:
      return detail ?? 'Los datos enviados no son válidos.';
    case 404:
      return 'La solicitud ya no existe.';
    case 409:
      // El backend explica exactamente por qué la transición no procede; ese texto es más útil
      // que cualquier mensaje genérico que pudiéramos escribir aquí.
      return detail ?? 'Esta solicitud ya fue decidida y no puede modificarse.';
    default:
      return 'Ocurrió un error inesperado. Inténtalo más tarde.';
  }
}

function readProblemDetail(error: HttpErrorResponse): string | null {
  const body: unknown = error.error;

  if (body && typeof body === 'object' && 'detail' in body) {
    const detail = (body as { detail: unknown }).detail;
    return typeof detail === 'string' ? detail : null;
  }

  return null;
}
