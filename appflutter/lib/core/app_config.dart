import 'package:flutter/foundation.dart';

// DECISIÓN DE DISEÑO: la URL base se resuelve con `defaultTargetPlatform` de flutter/foundation,
// no con `Platform.isAndroid` de dart:io.
// POR QUÉ: importar dart:io rompe la compilación en Flutter web, y esta app debe poder correr
// también en navegador y escritorio para poder demostrarla sin emulador.
// CONSECUENCIA: un único archivo decide contra qué host habla la app, en las cuatro plataformas.
class AppConfig {
  const AppConfig._();

  /// El emulador de Android expone la máquina anfitriona en 10.0.2.2: `localhost` dentro del
  /// emulador sería el propio dispositivo virtual, no el PC donde corre la API.
  static const String _androidEmulatorHost = 'http://10.0.2.2:5080';

  static const String _localHost = 'http://localhost:5080';

  static String get apiBaseUrl {
    if (kIsWeb) {
      return '$_localHost/api/v1';
    }

    if (defaultTargetPlatform == TargetPlatform.android) {
      return '$_androidEmulatorHost/api/v1';
    }

    return '$_localHost/api/v1';
  }

  static const Duration requestTimeout = Duration(seconds: 10);
}
