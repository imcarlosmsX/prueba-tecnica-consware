import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import {
  CATEGORY_LABELS,
  ReimbursementRequest,
  STATUS_LABELS,
} from '../../models/reimbursement-request.model';

// DECISIÓN DE DISEÑO: componente presentacional puro. Recibe la lista por `input` y emite la
// intención por `output`; no inyecta el servicio ni sabe qué pasa después de un clic.
// POR QUÉ: separar "qué se ve" de "qué se hace" permite que la página contenedora controle el
// estado de carga y la recarga, y hace la tabla reutilizable y trivial de probar.
// CONSECUENCIA: este archivo no contiene ninguna llamada HTTP ni regla de negocio.
@Component({
  selector: 'app-requests-table',
  imports: [CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './requests-table.component.html',
  styleUrl: './requests-table.component.css',
})
export class RequestsTableComponent {
  readonly requests = input.required<readonly ReimbursementRequest[]>();
  readonly busyRequestId = input<string | null>(null);

  readonly approveRequested = output<ReimbursementRequest>();
  readonly rejectRequested = output<ReimbursementRequest>();

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly categoryLabels = CATEGORY_LABELS;

  /// Solo las solicitudes Pendientes son accionables: el backend responde 409 a cualquier otra,
  /// y la tabla no debe ofrecer un botón que se sabe que va a fallar.
  protected isActionable(request: ReimbursementRequest): boolean {
    return request.status === 'Pending';
  }
}
