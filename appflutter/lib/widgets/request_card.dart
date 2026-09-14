import 'package:flutter/material.dart';

import '../core/formatters.dart';
import '../models/reimbursement_request.dart';
import 'status_chip.dart';

class RequestCard extends StatelessWidget {
  const RequestCard({super.key, required this.request});

  final ReimbursementRequest request;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Text(
                    Formatters.currency(request.amount),
                    style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
                  ),
                ),
                StatusChip(status: request.status),
              ],
            ),
            const SizedBox(height: 6),
            Text(request.category.label, style: theme.textTheme.labelMedium),
            const SizedBox(height: 8),
            Text(request.description, style: theme.textTheme.bodyMedium),
            const SizedBox(height: 10),
            Text(
              Formatters.dateTime(request.createdAtUtc),
              style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.outline),
            ),
            // El enunciado pide mostrar el motivo "si fue rechazada": solo aparece en ese caso.
            if (request.isRejected && request.rejectionReason != null)
              _RejectionReasonBanner(reason: request.rejectionReason!),
          ],
        ),
      ),
    );
  }
}

class _RejectionReasonBanner extends StatelessWidget {
  const _RejectionReasonBanner({required this.reason});

  final String reason;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(top: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: const Color(0xFFFBE0E0),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.info_outline, size: 18, color: Color(0xFF8C1D1D)),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Motivo del rechazo',
                  style: TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                    color: Color(0xFF8C1D1D),
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  reason,
                  style: const TextStyle(fontSize: 13, color: Color(0xFF8C1D1D)),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
