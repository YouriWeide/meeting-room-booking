import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { formatDayLabel, shortTime } from '../../date-utils';
import type { Occurrence } from '../../models/booking';

/** What the user chose to cancel. */
export type CancelScope = 'occurrence' | 'series';

/**
 * Asks whether to cancel one booking or the whole weekly series.
 */
@Component({
  selector: 'app-cancel-dialog',
  imports: [],
  templateUrl: './cancel-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CancelDialogComponent {
  private readonly modal = inject(NgbActiveModal);

  readonly occurrence = signal<Occurrence | null>(null);

  readonly roomName = signal('');

  confirm(scope: CancelScope): void {
    this.modal.close(scope);
  }

  dismiss(): void {
    this.modal.dismiss();
  }

  protected readonly formatDayLabel = formatDayLabel;

  protected readonly shortTime = shortTime;
}
