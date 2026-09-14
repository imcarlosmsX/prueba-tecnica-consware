import 'package:flutter/material.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:provider/provider.dart';

import 'providers/reimbursement_provider.dart';
import 'providers/session_provider.dart';
import 'screens/employee_selection_screen.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Carga los nombres de meses en español que usa Formatters.dateTime.
  await initializeDateFormatting('es');

  runApp(const ExpenseReimbursementApp());
}

/// Solo cableado de providers y tema: ninguna lógica de negocio vive aquí.
class ExpenseReimbursementApp extends StatelessWidget {
  const ExpenseReimbursementApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => SessionProvider()),
        ChangeNotifierProvider(create: (_) => ReimbursementProvider()),
      ],
      child: MaterialApp(
        title: 'Reembolsos',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF1B5E8C)),
          useMaterial3: true,
          cardTheme: const CardThemeData(elevation: 0.5),
        ),
        home: const EmployeeSelectionScreen(),
      ),
    );
  }
}
