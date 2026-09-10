import { ChangeDetectionStrategy, Component, computed, effect, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { formatDayLabel, shortTime, today } from '../../date-utils';
import type { Conflict, CreateReservationRequest, IsoDate, Room } from '../../models/booking';

/**
 * Books a room for a slot, once or every week for a number of weeks.
 *
 * Presentational. It collects the form and reports it; the container performs the
 * request and feeds the outcome back through `busy`, `conflicts` and `errorMessage`.
 */
@Component({
  selector: 'app-booking-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './booking-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingDialogComponent {
  readonly rooms = signal<readonly Room[]>([]);

  readonly bookedBy = signal('');

  readonly defaultRoomId = signal<number | null>(null);

  readonly defaultDate = signal<IsoDate | null>(null);

  readonly busy = signal(false);

  readonly conflicts = signal<readonly Conflict[]>([]);

  readonly errorMessage = signal<string | null>(null);

  readonly minDate = today();

  readonly submitted = output<CreateReservationRequest>();

  readonly form = new FormGroup({
    roomId: new FormControl<number | null>(null, { validators: [Validators.required] }),
    date: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    startTime: new FormControl('09:00', { nonNullable: true, validators: [Validators.required] }),
    endTime: new FormControl('10:00', { nonNullable: true, validators: [Validators.required] }),
    repeatWeekly: new FormControl(false, { nonNullable: true }),
    weeks: new FormControl(10, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(2), Validators.max(52)],
    }),
  });

  constructor(private readonly modal: NgbActiveModal) {
    // Pre-fill from the cell that was clicked, once the inputs are available.
    effect(() => {
      const roomId = this.defaultRoomId() ?? this.rooms()[0]?.id ?? null;
      this.form.controls.roomId.setValue(roomId, { emitEvent: false });
    });

    effect(() => {
      const date = this.defaultDate();

      if (date) {
        this.form.controls.date.setValue(date, { emitEvent: false });
      }
    });
  }

  get invalidTimes(): boolean {
    const { startTime, endTime } = this.form.getRawValue();
    return startTime !== '' && endTime !== '' && endTime <= startTime;
  }

  /** How many weeks would still be booked if the clashing ones are skipped. */
  readonly remainingAfterSkip = computed(() => {
    const requested = this.form.getRawValue().repeatWeekly ? this.form.getRawValue().weeks : 1;
    return requested - this.conflicts().length;
  });

  submit(skipConflicts = false): void {
    if (this.form.invalid || this.invalidTimes) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    this.submitted.emit({
      roomId: value.roomId!,
      bookedBy: this.bookedBy(),
      date: value.date,
      startTime: value.startTime,
      endTime: value.endTime,
      weeks: value.repeatWeekly ? value.weeks : 1,
      skipConflicts,
    });
  }

  dismiss(): void {
    this.modal.dismiss();
  }

  protected readonly formatDayLabel = formatDayLabel;

  protected readonly shortTime = shortTime;
}
