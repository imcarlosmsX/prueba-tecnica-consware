enum ApiErrorKind { network, timeout, validation, notFound, conflict, server }

// DECISIÓN DE DISEÑO: toda falla —de red, de timeout o de negocio— sale de la capa `core`
// convertida en esta excepción, con un mensaje ya redactado en español para el usuario.
// POR QUÉ: si el provider recibiera SocketException o http.Response, la UI acabaría teniendo
// que interpretar códigos HTTP y mostraríamos textos como "Connection refused" al empleado.
// CONSECUENCIA: los widgets solo muestran `message`; cambiar de paquete HTTP no toca la UI.
class ApiException implements Exception {
  const ApiException(this.kind, this.message, {this.fieldErrors});

  final ApiErrorKind kind;

  /// Mensaje listo para mostrar en pantalla, en español y sin jerga técnica.
  final String message;

  /// Errores por campo que devuelve el ProblemDetails del backend, cuando los hay.
  final Map<String, List<String>>? fieldErrors;

  factory ApiException.network() => const ApiException(
        ApiErrorKind.network,
        'No pudimos conectarnos al servidor. Revisa tu conexión a internet.',
      );

  factory ApiException.timeout() => const ApiException(
        ApiErrorKind.timeout,
        'El servidor está tardando en responder. Verifica tu conexión e inténtalo de nuevo.',
      );

  factory ApiException.notFound() => const ApiException(
        ApiErrorKind.notFound,
        'No encontramos esta solicitud.',
      );

  factory ApiException.conflict(String? detail) => ApiException(
        ApiErrorKind.conflict,
        detail ?? 'Esta solicitud ya fue procesada y no puede modificarse.',
      );

  factory ApiException.server() => const ApiException(
        ApiErrorKind.server,
        'Ocurrió un error inesperado. Inténtalo más tarde.',
      );

  @override
  String toString() => 'ApiException($kind): $message';
}
