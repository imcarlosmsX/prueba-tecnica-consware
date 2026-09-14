import 'package:flutter/foundation.dart';

import '../core/api_exception.dart';
import '../core/ui_state.dart';
import '../models/employee.dart';
import '../services/reimbursement_api_service.dart';

// DECISIÓN DE DISEÑO: el empleado actual se elige en una pantalla inicial y vive en este
// provider, sin autenticación.
// POR QUÉ: el enunciado dice explícitamente que el empleado puede identificarse "de forma
// sencilla (un nombre fijo o seleccionable al iniciar)" y que no hace falta autenticación.
// CONSECUENCIA: el día que haya login, solo cambia cómo se llena `selectedEmployee`; ninguna
// pantalla que lo consume se entera.
class SessionProvider extends ChangeNotifier {
  SessionProvider({ReimbursementApiService? apiService})
      : _apiService = apiService ?? ReimbursementApiService();

  final ReimbursementApiService _apiService;

  UiState _state = UiState.idle;
  List<Employee> _employees = const [];
  Employee? _selectedEmployee;
  String? _errorMessage;

  UiState get state => _state;
  List<Employee> get employees => _employees;
  Employee? get selectedEmployee => _selectedEmployee;
  String? get errorMessage => _errorMessage;
  bool get hasSelectedEmployee => _selectedEmployee != null;

  Future<void> loadEmployees() async {
    _state = UiState.loading;
    _errorMessage = null;
    notifyListeners();

    try {
      _employees = await _apiService.fetchEmployees();
      _state = UiState.success;
    } on ApiException catch (exception) {
      _errorMessage = exception.message;
      _state = UiState.failure;
    } catch (_) {
      // Ninguna excepción inesperada debe escapar del provider hacia el árbol de widgets.
      _errorMessage = 'Ocurrió un error inesperado. Inténtalo más tarde.';
      _state = UiState.failure;
    }

    notifyListeners();
  }

  void selectEmployee(Employee employee) {
    _selectedEmployee = employee;
    notifyListeners();
  }

  void signOut() {
    _selectedEmployee = null;
    notifyListeners();
  }
}
