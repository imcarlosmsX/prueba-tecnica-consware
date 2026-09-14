import 'package:flutter/material.dart';

import '../models/reimbursement_status.dart';

/// Traduce el estado a color e icono. Es el único lugar donde se decide cómo se ve un estado,
/// así que agregar uno nuevo al backend se refleja aquí y en ningún otro widget.
class StatusChip extends StatelessWidget {
  const StatusChip({super.key, required this.status});

  final ReimbursementStatus status;

  @override
  Widget build(BuildContext context) {
    final (background, foreground, icon) = switch (status) {
      ReimbursementStatus.approved => (
          const Color(0xFFDCF5E3),
          const Color(0xFF11603A),
          Icons.check_circle_outline,
        ),
      ReimbursementStatus.rejected => (
          const Color(0xFFFBE0E0),
          const Color(0xFF8C1D1D),
          Icons.cancel_outlined,
        ),
      ReimbursementStatus.pending => (
          const Color(0xFFFDF0D5),
          const Color(0xFF7A5200),
          Icons.schedule,
        ),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 15, color: foreground),
          const SizedBox(width: 5),
          Text(
            status.label,
            style: TextStyle(
              color: foreground,
              fontSize: 12,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}
