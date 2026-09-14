import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ReimbursementRequest } from '../../models/reimbursement-request.model';

// DECISIÓN DE DISEÑO: Reactive Forms y no ngModel para capturar el motivo del rechazo.
// POR QUÉ: el enunciado exige que el motivo sea obligatorio. Con Reactive Forms la validación
// es un objeto que el componente puede consultar (`form.invalid`) y probar sin renderizar; con
// Template-Driven quedaría repartida entre atributos del HTML.
// CONSECUENCIA: es imposible enviar un rechazo sin motivo desde la interfaz, y el estado de
// validez se lee en una sola expresión.
@Component({
  selector: 'app-reject-dialog',
  imports: [CurrencyPipe, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './reject-dialog.component.html',
  styleUrl: './reject-dialog.component.css',
})
export class RejectDialogComponent {
  readonly request = input.required<ReimbursementRequest>();
  readonly submitting = input(false);

  readonly confirmed = output<string>();
  readonly cancelled = output<void>();

  private readonly formBuilder = inject(FormBuilder);

  protected readonly maxReasonLength = 500;

  // El patrón \S exige al menos un carácter que no sea espacio: sin él, "   " pasaría
  // Validators.required y el backend tendría que rechazar con un 400 algo que la interfaz
  // nunca debió permitir enviar.
  protected readonly form = this.formBuilder.nonNullable.group({
    reason: [
      '',
      [Validators.required, Validators.pattern(/\S/), Validators.maxLength(500)],
    ],
  });

  protected get reason() {
    return this.form.controls.reason;
  }

  protected submit(): void {
    // Marcar como "tocado" hace visible el mensaje de error cuando el usuario intenta enviar
    // sin haber escrito nada, en vez de dejar el botón inerte sin explicación.
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.confirmed.emit(this.reason.value.trim());
  }
}
