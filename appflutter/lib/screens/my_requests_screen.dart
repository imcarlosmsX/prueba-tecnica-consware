import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/ui_state.dart';
import '../providers/reimbursement_provider.dart';
import '../providers/session_provider.dart';
import '../widgets/empty_view.dart';
import '../widgets/error_view.dart';
import '../widgets/request_card.dart';
import 'create_request_screen.dart';

class MyRequestsScreen extends StatefulWidget {
  const MyRequestsScreen({super.key});

  @override
  State<MyRequestsScreen> createState() => _MyRequestsScreenState();
}

class _MyRequestsScreenState extends State<MyRequestsScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadRequests());
  }

  Future<void> _loadRequests() {
    final employee = context.read<SessionProvider>().selectedEmployee;

    if (employee == null) {
      return Future.value();
    }

    return context.read<ReimbursementProvider>().loadMyRequests(employee.id);
  }

  Future<void> _openCreateScreen() async {
    final created = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => const CreateRequestScreen()),
    );

    if (created == true && mounted) {
      await _loadRequests();
    }
  }

  @override
  Widget build(BuildContext context) {
    final session = context.watch<SessionProvider>();
    final reimbursements = context.watch<ReimbursementProvider>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis solicitudes'),
        actions: [
          IconButton(
            tooltip: 'Cambiar de empleado',
            icon: const Icon(Icons.logout),
            onPressed: () {
              session.signOut();
              Navigator.of(context).pop();
            },
          ),
        ],
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(28),
          child: Padding(
            padding: const EdgeInsets.only(left: 16, bottom: 12),
            child: Align(
              alignment: Alignment.centerLeft,
              child: Text(
                session.selectedEmployee?.fullName ?? '',
                style: Theme.of(context).textTheme.bodyMedium,
              ),
            ),
          ),
        ),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _openCreateScreen,
        icon: const Icon(Icons.add),
        label: const Text('Registrar gasto'),
      ),
      body: switch (reimbursements.state) {
        UiState.idle || UiState.loading => const Center(child: CircularProgressIndicator()),
        UiState.failure => ErrorView(
            message: reimbursements.errorMessage ?? 'No pudimos cargar tus solicitudes.',
            onRetry: _loadRequests,
          ),
        UiState.success => reimbursements.hasRequests
            ? _RequestList(onRefresh: _loadRequests)
            : RefreshIndicator(
                onRefresh: _loadRequests,
                // El estado vacío va dentro de una lista desplazable para que el gesto de
                // "deslizar para refrescar" funcione también cuando no hay nada que mostrar.
                child: ListView(
                  children: const [
                    SizedBox(height: 120),
                    EmptyView(
                      title: 'Aún no tienes solicitudes',
                      message: 'Toca "Registrar gasto" para crear tu primera solicitud de reembolso.',
                      icon: Icons.receipt_long_outlined,
                    ),
                  ],
                ),
              ),
      },
    );
  }
}

class _RequestList extends StatelessWidget {
  const _RequestList({required this.onRefresh});

  final Future<void> Function() onRefresh;

  @override
  Widget build(BuildContext context) {
    final requests = context.watch<ReimbursementProvider>().requests;

    return RefreshIndicator(
      onRefresh: onRefresh,
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 96),
        itemCount: requests.length,
        itemBuilder: (context, index) => RequestCard(request: requests[index]),
      ),
    );
  }
}
