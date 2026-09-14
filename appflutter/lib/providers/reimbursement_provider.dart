import 'package:flutter/foundation.dart';

import '../core/api_exception.dart';
import '../core/ui_state.dart';
import '../models/expense_category.dart';
import '../models/reimbursement_request.dart';
import '../services/reimbursement_api_service.dart';

// DECISIÓN DE DISEÑO: el estado de las solicitudes vive aquí, en un ChangeNotifier, y ningún
// widget llama nunca al servicio ni al cliente HTTP.
// POR QUÉ: elegí Provider sobre Bloc o Riverpod por relación costo/beneficio: separa UI de
// lógica con la mínima ceremonia. En una app que crece usaría Bloc por sus estados explícitos,
// pero su boilerplate no se amortiza en dos pantallas.
// CONSECUENCIA: la lógica de carga y envío se puede leer entera en un archivo, y las pantallas
// solo dibujan lo que este objeto expone.
class ReimbursementProvider extends ChangeNotifier {
  ReimbursementProvider({ReimbursementApiService? apiService})
      : _apiService = apiService ?? ReimbursementApiService();

  final ReimbursementApiService _apiService;

  UiState _state = UiState.idle;
  List<ReimbursementRequest> _requests = const [];
  String? _errorMessage;
  bool _isSubmitting = false;
  String? _submissionError;

  UiState get state => _state;
  List<ReimbursementRequest> get requests => _requests;
  String? get errorMessage => _errorMessage;

  /// Mientras es true el botón de envío se deshabilita, para que un doble toque no cree dos
  /// solicitudes idénticas.
  bool get isSubmitting => _isSubmitting;

  /// Error del último envío, para mostrarlo en un SnackBar sin bloquear la pantalla.
  String? get submissionError => _submissionError;

  bool get hasRequests => _requests.isNotEmpty;

  Future<void> loadMyRequests(String employeeId) async {
    _state = UiState.loading;
    _errorMessage = null;
    notifyListeners();

    try {
      _requests = await _apiService.fetchRequestsByEmployee(employeeId);
      _state = UiState.success;
    } on ApiException catch (exception) {
      _errorMessage = exception.message;
      _state = UiState.failure;
    } catch (_) {
      _errorMessage = 'Ocurrió un error inesperado. Inténtalo más tarde.';
      _state = UiState.failure;
    }

    notifyListeners();
  }

  /// Devuelve la solicitud creada si todo salió bien, o null si falló. El mensaje de error
  /// queda en [submissionError] para que la pantalla lo muestre en un SnackBar.
  Future<ReimbursementRequest?> submitRequest({
    required String employeeId,
    required double amount,
    required ExpenseCategory category,
    required String description,
  }) async {
    if (_isSubmitting) {
      return null;
    }

    _isSubmitting = true;
    _submissionError = null;
    notifyListeners();

    try {
      final created = await _apiService.createRequest(
        employeeId: employeeId,
        amount: amount,
        category: category,
        description: description,
      );

      _requests = [created, ..._requests];
      _state = UiState.success;
      return created;
    } on ApiException catch (exception) {
      _submissionError = exception.message;
      return null;
    } catch (_) {
      _submissionError = 'Ocurrió un error inesperado. Inténtalo más tarde.';
      return null;
    } finally {
      _isSubmitting = false;
      notifyListeners();
    }
  }

  void clearSubmissionError() {
    _submissionError = null;
  }
}
