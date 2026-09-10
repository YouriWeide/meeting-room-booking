import { ChangeDetectionStrategy, Component, computed, effect, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { formatDayLabel, shortTime, today } from '../../date-utils';
import type { Conflict, CreateReservationRequest, IsoDate, Room } from '../../models/booking';

/**
 * Books a room for a slot, once or every week for a number of weeks.
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
    weeks: new FormControl(2, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(2), Validators.max(52)],
    }),
  });

  readonly canSubmit = computed(() => !this.busy() && this.conflicts().length === 0);

  constructor(private readonly modal: NgbActiveModal) {
    // A conflict describes one specific request. The moment any field changes it is
    // about something the user is no longer asking for, so it goes - which also
    // re-enables Reserveren for the new values.
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      if (this.conflicts().length > 0) {
        this.conflicts.set([]);
      }

      if (this.errorMessage() !== null) {
        this.errorMessage.set(null);
      }
    });

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

  private readonly requestedSlots = signal(1);

  /** How many slots would still be booked if the clashing ones are skipped. */
  readonly remainingAfterSkip = computed(() => this.requestedSlots() - this.conflicts().length);

  /**
   * Reads correctly whether one slot was asked for or multiple.
   */
  readonly conflictSummary = computed(() => {
    const taken = this.conflicts().length;
    const requested = this.requestedSlots();

    if (requested === 1) {
      return 'Dit tijdslot is al bezet.';
    }

    if (taken >= requested) {
      return `Alle ${requested} gevraagde tijdsloten zijn al bezet.`;
    }

    return `${taken} van de ${requested} gevraagde tijdsloten ${taken === 1 ? 'is' : 'zijn'} al bezet.`;
  });

  submit(skipConflicts = false): void {
    if (this.form.invalid || this.invalidTimes) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.requestedSlots.set(value.repeatWeekly ? value.weeks : 1);

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
