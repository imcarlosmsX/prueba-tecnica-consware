import 'expense_category.dart';
import 'reimbursement_status.dart';

// DECISIÓN DE DISEÑO: todos los campos son `final` y el parseo de JSON es defensivo, con valor
// por defecto en cada campo que pudiera faltar.
// POR QUÉ: un `json['x'] as String` sin red de seguridad lanza en tiempo de ejecución si el
// backend cambia un nombre o envía null, y el usuario vería la pantalla roja de Flutter.
// CONSECUENCIA: un cambio en el contrato degrada un dato, no tumba la pantalla.
class ReimbursementRequest {
  const ReimbursementRequest({
    required this.id,
    required this.employeeId,
    required this.employeeName,
    required this.amount,
    required this.currency,
    required this.category,
    required this.description,
    required this.status,
    required this.createdAtUtc,
    this.rejectionReason,
    this.decidedAtUtc,
  });

  final String id;
  final String employeeId;
  final String employeeName;
  final double amount;
  final String currency;
  final ExpenseCategory category;
  final String description;
  final ReimbursementStatus status;
  final String? rejectionReason;
  final DateTime createdAtUtc;
  final DateTime? decidedAtUtc;

  bool get isRejected => status == ReimbursementStatus.rejected;

  factory ReimbursementRequest.fromJson(Map<String, dynamic> json) {
    return ReimbursementRequest(
      id: json['id'] as String? ?? '',
      employeeId: json['employeeId'] as String? ?? '',
      employeeName: json['employeeName'] as String? ?? '',
      amount: (json['amount'] as num?)?.toDouble() ?? 0,
      currency: json['currency'] as String? ?? 'COP',
      category: ExpenseCategory.fromWire(json['category'] as String?),
      description: json['description'] as String? ?? '',
      status: ReimbursementStatus.fromWire(json['status'] as String?),
      rejectionReason: json['rejectionReason'] as String?,
      createdAtUtc: DateTime.tryParse(json['createdAtUtc'] as String? ?? '')?.toLocal() ??
          DateTime.now(),
      decidedAtUtc: DateTime.tryParse(json['decidedAtUtc'] as String? ?? '')?.toLocal(),
    );
  }
}
