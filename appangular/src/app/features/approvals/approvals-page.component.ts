import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';

import { ReimbursementApiService } from '../../core/reimbursement-api.service';
import {
  ReimbursementRequest,
  ReimbursementStatus,
  STATUS_LABELS,
} from '../../models/reimbursement-request.model';
import { RejectDialogComponent } from './reject-dialog.component';
import { RequestsTableComponent } from './requests-table.component';

type StatusFilter = ReimbursementStatus | 'All';

// DECISIÓN DE DISEÑO: componente contenedor. Posee el estado con signals, habla con el servicio
// y delega el dibujo a dos componentes presentacionales.
// POR QUÉ: un signal es una variable que avisa a la vista cuando cambia, sin necesidad de
// suscripciones manuales ni de recordar llamar a detectChanges. Con OnPush en toda la pantalla,
// Angular solo re-renderiza lo que realmente cambió.
// CONSECUENCIA: la tabla y el diálogo no saben que existe una API, y este archivo no contiene
// una sola línea de HTML de presentación.
@Component({
  selector: 'app-approvals-page',
  imports: [RequestsTableComponent, RejectDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './approvals-page.component.html',
  styleUrl: './approvals-page.component.css',
})
export class ApprovalsPageComponent implements OnInit {
  private readonly api = inject(ReimbursementApiService);

  protected readonly requests = signal<readonly ReimbursementRequest[]>([]);
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly feedback = signal<string | null>(null);

  /// Id de la solicitud que se está decidiendo: deshabilita los botones y evita doble clic.
  protected readonly busyRequestId = signal<string | null>(null);

  /// El enunciado pide que el filtro por defecto sean las Pendientes, que es lo que el
  /// aprobador necesita resolver al abrir el panel.
  protected readonly statusFilter = signal<StatusFilter>('Pending');

  protected readonly requestToReject = signal<ReimbursementRequest | null>(null);

  /// El texto del estado vacío se arma aquí y no en la plantilla porque 'All' no es un estado
  /// del dominio y no tiene etiqueta: el compilador lo señala si se intenta traducir.
  protected readonly emptyMessage = computed(() => {
    const filter = this.statusFilter();

    return filter === 'All'
      ? 'No hay solicitudes registradas.'
      : `No hay solicitudes en estado ${STATUS_LABELS[filter]}.`;
  });

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly filterOptions: readonly StatusFilter[] = [
    'Pending',
    'Approved',
    'Rejected',
    'All',
  ];

  ngOnInit(): void {
    this.loadRequests();
  }

  protected changeFilter(filter: StatusFilter): void {
    this.statusFilter.set(filter);
    this.loadRequests();
  }

  protected loadRequests(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    const filter = this.statusFilter();

    this.api.getRequests(filter === 'All' ? null : filter).subscribe({
      next: (requests) => {
        this.requests.set(requests);
        this.loading.set(false);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.loading.set(false);
      },
    });
  }

  protected approve(request: ReimbursementRequest): void {
    this.busyRequestId.set(request.id);

    this.api.approve(request.id).subscribe({
      next: () => this.onDecisionApplied(`Solicitud de ${request.employeeName} aprobada.`),
      error: (error: Error) => this.onDecisionFailed(error),
    });
  }

  protected openRejectDialog(request: ReimbursementRequest): void {
    this.requestToReject.set(request);
  }

  protected closeRejectDialog(): void {
    this.requestToReject.set(null);
  }

  protected confirmReject(reason: string): void {
    const request = this.requestToReject();

    if (!request) {
      return;
    }

    this.busyRequestId.set(request.id);

    this.api.reject(request.id, reason).subscribe({
      next: () => {
        this.closeRejectDialog();
        this.onDecisionApplied(`Solicitud de ${request.employeeName} rechazada.`);
      },
      error: (error: Error) => {
        this.closeRejectDialog();
        this.onDecisionFailed(error);
      },
    });
  }

  /// Tras cada decisión se recarga la tabla desde el servidor en vez de editar la fila en
  /// memoria: así el panel siempre refleja el estado real, incluso si otro aprobador decidió
  /// esa misma solicitud mientras esta pantalla estaba abierta.
  private onDecisionApplied(message: string): void {
    this.busyRequestId.set(null);
    this.feedback.set(message);
    this.loadRequests();
  }

  private onDecisionFailed(error: Error): void {
    this.busyRequestId.set(null);
    this.errorMessage.set(error.message);
    // Se recarga igual: un 409 significa que alguien más ya la decidió, y la tabla debe
    // mostrar el estado actual en vez de seguir ofreciendo botones que ya no aplican.
    this.loadRequests();
  }

  protected dismissFeedback(): void {
    this.feedback.set(null);
  }
}
