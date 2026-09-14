import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import 'api_config_messages.dart';
import 'api_exception.dart';
import 'app_config.dart';

// DECISIÓN DE DISEÑO: único punto de la app que conoce el paquete http, los timeouts y el
// formato de error del backend (RFC 7807 ProblemDetails).
// POR QUÉ: si cada servicio hiciera su propia llamada, el timeout habría que recordarlo en cada
// una y el día que un endpoint devuelva un error nuevo habría que actualizar varios sitios.
// CONSECUENCIA: todo error de red o de negocio llega a la UI como ApiException traducida.
class ApiClient {
  ApiClient({http.Client? httpClient}) : _httpClient = httpClient ?? http.Client();

  final http.Client _httpClient;

  Future<dynamic> get(String path, {Map<String, String>? queryParameters}) =>
      _send(() => _httpClient.get(_buildUri(path, queryParameters), headers: _headers));

  Future<dynamic> post(String path, {Object? body}) => _send(
        () => _httpClient.post(
          _buildUri(path, null),
          headers: _headers,
          body: body == null ? null : jsonEncode(body),
        ),
      );

  static const Map<String, String> _headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  };

  Uri _buildUri(String path, Map<String, String>? queryParameters) {
    final uri = Uri.parse('${AppConfig.apiBaseUrl}$path');

    if (queryParameters == null || queryParameters.isEmpty) {
      return uri;
    }

    return uri.replace(queryParameters: queryParameters);
  }

  Future<dynamic> _send(Future<http.Response> Function() request) async {
    final http.Response response;

    try {
      response = await request().timeout(AppConfig.requestTimeout);
    } on TimeoutException {
      throw ApiException.timeout();
    } catch (_) {
      // SocketException, ClientException, HandshakeException y equivalentes en web: para el
      // usuario todas significan lo mismo, que no hubo forma de hablar con el servidor.
      throw ApiException.network();
    }

    return _parseResponse(response);
  }

  dynamic _parseResponse(http.Response response) {
    final statusCode = response.statusCode;

    if (statusCode >= 200 && statusCode < 300) {
      if (response.body.isEmpty) {
        return null;
      }
      return jsonDecode(utf8.decode(response.bodyBytes));
    }

    throw _toApiException(statusCode, response.bodyBytes);
  }

  ApiException _toApiException(int statusCode, List<int> bodyBytes) {
    final problem = _tryDecodeProblemDetails(bodyBytes);

    return switch (statusCode) {
      400 || 422 => ApiException(
          ApiErrorKind.validation,
          problem?.detail ?? ApiConfigMessages.invalidData,
          fieldErrors: problem?.fieldErrors,
        ),
      404 => ApiException.notFound(),
      409 => ApiException.conflict(problem?.detail),
      _ => ApiException.server(),
    };
  }

  _ProblemDetails? _tryDecodeProblemDetails(List<int> bodyBytes) {
    try {
      final decoded = jsonDecode(utf8.decode(bodyBytes));
      if (decoded is! Map<String, dynamic>) {
        return null;
      }
      return _ProblemDetails.fromJson(decoded);
    } catch (_) {
      // Un cuerpo que no es ProblemDetails (un proxy, un HTML de error) no debe tumbar la app:
      // se cae al mensaje genérico del código de estado.
      return null;
    }
  }
}

class _ProblemDetails {
  const _ProblemDetails({this.detail, this.fieldErrors});

  final String? detail;
  final Map<String, List<String>>? fieldErrors;

  factory _ProblemDetails.fromJson(Map<String, dynamic> json) {
    final rawErrors = json['errors'];
    Map<String, List<String>>? fieldErrors;

    if (rawErrors is Map<String, dynamic>) {
      fieldErrors = rawErrors.map(
        (field, messages) => MapEntry(
          field,
          messages is List ? messages.map((message) => message.toString()).toList() : <String>[],
        ),
      );
    }

    return _ProblemDetails(
      detail: json['detail'] as String?,
      fieldErrors: fieldErrors,
    );
  }
}
