import { ChangeDetectionStrategy, Component } from '@angular/core';

import { ApprovalsPageComponent } from './features/approvals/approvals-page.component';

@Component({
  selector: 'app-root',
  imports: [ApprovalsPageComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '<app-approvals-page />',
})
export class App {}
