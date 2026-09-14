enum ReimbursementStatus {
  pending('Pending', 'Pendiente'),
  approved('Approved', 'Aprobada'),
  rejected('Rejected', 'Rechazada');

  const ReimbursementStatus(this.wireValue, this.label);

  /// Valor tal como viaja en el JSON de la API.
  final String wireValue;

  /// Texto que ve el empleado en pantalla.
  final String label;

  /// Parseo defensivo: si el backend añadiera un estado nuevo, la app lo muestra como
  /// Pendiente en vez de lanzar una excepción y romper toda la pantalla de listado.
  static ReimbursementStatus fromWire(String? value) {
    return ReimbursementStatus.values.firstWhere(
      (status) => status.wireValue == value,
      orElse: () => ReimbursementStatus.pending,
    );
  }
}
