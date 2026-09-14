import 'package:intl/intl.dart';

/// Formato de moneda y fecha en convención colombiana. Centralizado para que un monto no se
/// vea distinto en la lista y en el detalle.
class Formatters {
  const Formatters._();

  // Se formatea solo el número y el símbolo se antepone a mano: NumberFormat.currency con
  // locale es_CO lo coloca al final ("500.000 $"), que no es como se escribe un monto en
  // Colombia.
  static final NumberFormat _amount = NumberFormat('#,##0', 'es_CO');

  static final DateFormat _date = DateFormat('d MMM y, h:mm a', 'es');

  static String currency(double amount) => '\$${_amount.format(amount)}';

  static String dateTime(DateTime value) => _date.format(value);
}
