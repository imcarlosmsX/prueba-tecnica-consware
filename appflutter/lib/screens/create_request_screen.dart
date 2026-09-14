import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';

import '../models/expense_category.dart';
import '../models/reimbursement_status.dart';
import '../providers/reimbursement_provider.dart';
import '../providers/session_provider.dart';

// DECISIÓN DE DISEÑO: el formulario valida formato (número, campos presentes, longitud) pero
// NO decide si la solicitud se auto-aprueba: eso lo resuelve el backend.
// POR QUÉ: duplicar el umbral de $500.000 aquí crearía dos fuentes de verdad, y el día que la
// empresa lo cambie la app mentiría hasta que alguien la recompile.
// CONSECUENCIA: el estado resultante siempre llega del servidor y la app solo lo muestra.
class CreateRequestScreen extends StatefulWidget {
  const CreateRequestScreen({super.key});

  @override
  State<CreateRequestScreen> createState() => _CreateRequestScreenState();
}

class _CreateRequestScreenState extends State<CreateRequestScreen> {
  final _formKey = GlobalKey<FormState>();
  final _amountController = TextEditingController();
  final _descriptionController = TextEditingController();

  ExpenseCategory _category = ExpenseCategory.meals;

  static const int _maxDescriptionLength = 500;

  @override
  void dispose() {
    _amountController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  String? _validateAmount(String? value) {
    final text = value?.trim() ?? '';

    if (text.isEmpty) {
      return 'Ingresa el monto del gasto.';
    }

    final amount = double.tryParse(text.replaceAll(',', '.'));

    if (amount == null) {
      return 'El monto debe ser un número.';
    }

    if (amount <= 0) {
      return 'El monto debe ser mayor que cero.';
    }

    return null;
  }

  String? _validateDescription(String? value) {
    final text = value?.trim() ?? '';

    if (text.isEmpty) {
      return 'Describe brevemente el gasto.';
    }

    if (text.length > _maxDescriptionLength) {
      return 'La descripción no puede superar $_maxDescriptionLength caracteres.';
    }

    return null;
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final employee = context.read<SessionProvider>().selectedEmployee;

    if (employee == null) {
      return;
    }

    final provider = context.read<ReimbursementProvider>();

    final created = await provider.submitRequest(
      employeeId: employee.id,
      amount: double.parse(_amountController.text.trim().replaceAll(',', '.')),
      category: _category,
      description: _descriptionController.text.trim(),
    );

    if (!mounted) {
      return;
    }

    if (created == null) {
      _showSnackBar(
        provider.submissionError ?? 'No pudimos registrar tu solicitud.',
        isError: true,
      );
      provider.clearSubmissionError();
      return;
    }

    // El mensaje refleja lo que decidió el backend, no lo que la app supone.
    _showSnackBar(
      created.status == ReimbursementStatus.approved
          ? 'Solicitud aprobada automáticamente.'
          : 'Solicitud registrada. Queda pendiente de aprobación.',
    );

    Navigator.of(context).pop(true);
  }

  void _showSnackBar(String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? Theme.of(context).colorScheme.error : null,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final isSubmitting = context.watch<ReimbursementProvider>().isSubmitting;

    return Scaffold(
      appBar: AppBar(title: const Text('Registrar gasto')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _amountController,
              enabled: !isSubmitting,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              inputFormatters: [
                FilteringTextInputFormatter.allow(RegExp(r'[0-9.,]')),
              ],
              decoration: const InputDecoration(
                labelText: 'Monto',
                prefixText: r'$ ',
                helperText: 'En pesos colombianos',
                border: OutlineInputBorder(),
              ),
              validator: _validateAmount,
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<ExpenseCategory>(
              initialValue: _category,
              decoration: const InputDecoration(
                labelText: 'Categoría',
                border: OutlineInputBorder(),
              ),
              items: ExpenseCategory.values
                  .map(
                    (category) => DropdownMenuItem(
                      value: category,
                      child: Text(category.label),
                    ),
                  )
                  .toList(),
              onChanged: isSubmitting
                  ? null
                  : (category) => setState(() => _category = category ?? ExpenseCategory.other),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _descriptionController,
              enabled: !isSubmitting,
              maxLines: 3,
              maxLength: _maxDescriptionLength,
              decoration: const InputDecoration(
                labelText: 'Descripción',
                hintText: 'Ej. Almuerzo con cliente ACME',
                border: OutlineInputBorder(),
                alignLabelWithHint: true,
              ),
              validator: _validateDescription,
            ),
            const SizedBox(height: 24),
            FilledButton(
              // Deshabilitar el botón mientras se envía evita que un doble toque cree dos
              // solicitudes idénticas: la guarda está en el provider y se refleja aquí.
              onPressed: isSubmitting ? null : _submit,
              style: FilledButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
              child: isSubmitting
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Enviar solicitud'),
            ),
          ],
        ),
      ),
    );
  }
}
