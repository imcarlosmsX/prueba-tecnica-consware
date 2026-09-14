class Employee {
  const Employee({required this.id, required this.fullName});

  final String id;
  final String fullName;

  factory Employee.fromJson(Map<String, dynamic> json) => Employee(
        id: json['id'] as String? ?? '',
        fullName: json['fullName'] as String? ?? 'Sin nombre',
      );
}
