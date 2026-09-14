import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/ui_state.dart';
import '../models/employee.dart';
import '../providers/session_provider.dart';
import '../widgets/empty_view.dart';
import '../widgets/error_view.dart';
import 'my_requests_screen.dart';

/// Sustituye al login: el enunciado permite identificar al empleado "de forma sencilla
/// (un nombre fijo o seleccionable al iniciar)" y no pide autenticación.
class EmployeeSelectionScreen extends StatefulWidget {
  const EmployeeSelectionScreen({super.key});

  @override
  State<EmployeeSelectionScreen> createState() => _EmployeeSelectionScreenState();
}

class _EmployeeSelectionScreenState extends State<EmployeeSelectionScreen> {
  @override
  void initState() {
    super.initState();
    // La carga se dispara después del primer frame: llamar a un provider que notifica durante
    // build lanza una excepción de Flutter.
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadEmployees());
  }

  Future<void> _loadEmployees() => context.read<SessionProvider>().loadEmployees();

  void _selectEmployee(BuildContext context, Employee employee) {
    context.read<SessionProvider>().selectEmployee(employee);

    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const MyRequestsScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    final session = context.watch<SessionProvider>();

    return Scaffold(
      appBar: AppBar(title: const Text('¿Quién eres?')),
      body: switch (session.state) {
        UiState.idle || UiState.loading => const Center(child: CircularProgressIndicator()),
        UiState.failure => ErrorView(
            message: session.errorMessage ?? 'No pudimos cargar los empleados.',
            onRetry: _loadEmployees,
          ),
        UiState.success => session.employees.isEmpty
            ? const EmptyView(
                title: 'No hay empleados registrados',
                message: 'Verifica que la API tenga datos de ejemplo cargados.',
                icon: Icons.people_outline,
              )
            : _EmployeeList(onSelected: (employee) => _selectEmployee(context, employee)),
      },
    );
  }
}

class _EmployeeList extends StatelessWidget {
  const _EmployeeList({required this.onSelected});

  final void Function(Employee employee) onSelected;

  @override
  Widget build(BuildContext context) {
    final employees = context.watch<SessionProvider>().employees;

    return ListView.separated(
      padding: const EdgeInsets.all(16),
      itemCount: employees.length,
      separatorBuilder: (_, _) => const SizedBox(height: 8),
      itemBuilder: (context, index) {
        final employee = employees[index];

        return Card(
          margin: EdgeInsets.zero,
          child: ListTile(
            leading: CircleAvatar(child: Text(employee.fullName.characters.first)),
            title: Text(employee.fullName),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => onSelected(employee),
          ),
        );
      },
    );
  }
}
