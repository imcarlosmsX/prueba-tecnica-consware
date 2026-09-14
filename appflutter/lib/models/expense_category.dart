enum ExpenseCategory {
  meals('Meals', 'Alimentación'),
  transportation('Transportation', 'Transporte'),
  officeSupplies('OfficeSupplies', 'Papelería'),
  accommodation('Accommodation', 'Alojamiento'),
  other('Other', 'Otros');

  const ExpenseCategory(this.wireValue, this.label);

  final String wireValue;

  final String label;

  static ExpenseCategory fromWire(String? value) {
    return ExpenseCategory.values.firstWhere(
      (category) => category.wireValue == value,
      orElse: () => ExpenseCategory.other,
    );
  }
}
