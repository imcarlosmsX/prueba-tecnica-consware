import '../core/api_client.dart';
import '../models/employee.dart';
import '../models/expense_category.dart';
import '../models/reimbursement_request.dart';

/// Único lugar de la app que conoce las rutas de la API. Los providers piden datos de negocio
/// ("mis solicitudes"), no URLs.
class ReimbursementApiService {
  ReimbursementApiService({ApiClient? apiClient}) : _apiClient = apiClient ?? ApiClient();

  final ApiClient _apiClient;

  Future<List<Employee>> fetchEmployees() async {
    final data = await _apiClient.get('/employees');

    return _asList(data).map(Employee.fromJson).toList();
  }

  Future<List<ReimbursementRequest>> fetchRequestsByEmployee(String employeeId) async {
    final data = await _apiClient.get(
      '/reimbursement-requests',
      queryParameters: {'employeeId': employeeId},
    );

    return _asList(data).map(ReimbursementRequest.fromJson).toList();
  }

  Future<ReimbursementRequest> createRequest({
    required String employeeId,
    required double amount,
    required ExpenseCategory category,
    required String description,
  }) async {
    final data = await _apiClient.post(
      '/reimbursement-requests',
      body: {
        'employeeId': employeeId,
        'amount': amount,
        'category': category.wireValue,
        'description': description,
      },
    );

    return ReimbursementRequest.fromJson(data as Map<String, dynamic>);
  }

  List<Map<String, dynamic>> _asList(dynamic data) {
    if (data is! List) {
      return const [];
    }

    return data.whereType<Map<String, dynamic>>().toList();
  }
}
