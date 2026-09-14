export type ReimbursementStatus = 'Pending' | 'Approved' | 'Rejected';

export type ExpenseCategory =
  | 'Meals'
  | 'Transportation'
  | 'OfficeSupplies'
  | 'Accommodation'
  | 'Other';

// DECISIÓN DE DISEÑO: los estados y categorías son union types de strings, no enums de
// TypeScript ni `string` a secas.
// POR QUÉ: el backend serializa los enums como texto, así que el union type describe
// exactamente lo que llega por el cable y el compilador detecta un `case` que falte en un
// switch. Un `string` suelto dejaría pasar cualquier valor inventado.
// CONSECUENCIA: agregar un estado en el backend rompe la compilación aquí hasta que se maneje.
export interface ReimbursementRequest {
  readonly id: string;
  readonly employeeId: string;
  readonly employeeName: string;
  readonly amount: number;
  readonly currency: string;
  readonly category: ExpenseCategory;
  readonly description: string;
  readonly status: ReimbursementStatus;
  readonly rejectionReason: string | null;
  readonly createdAtUtc: string;
  readonly decidedAtUtc: string | null;
}

export const STATUS_LABELS: Record<ReimbursementStatus, string> = {
  Pending: 'Pendiente',
  Approved: 'Aprobada',
  Rejected: 'Rechazada',
};

export const CATEGORY_LABELS: Record<ExpenseCategory, string> = {
  Meals: 'Alimentación',
  Transportation: 'Transporte',
  OfficeSupplies: 'Papelería',
  Accommodation: 'Alojamiento',
  Other: 'Otros',
};
