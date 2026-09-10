import { ChangeDetectionStrategy, Component, computed, output, signal } from '@angular/core';
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

  readonly busy = signal(false);

  readonly conflicts = signal<readonly Conflict[]>([]);

  readonly errorMessage = signal<string | null>(null);

  /** Read on each render rather than captured once, so it is still right at midnight. */
  get minDate(): IsoDate {
    return today();
  }

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

  /**
   * The form's value as a signal. A FormGroup is not reactive on its own, so anything
   * derived from it has to be fed by valueChanges or it silently goes stale.
   */
  private readonly formValue = signal(this.form.getRawValue());

  readonly invalidTimes = computed(() => {
    const { startTime, endTime } = this.formValue();
    return startTime !== '' && endTime !== '' && endTime <= startTime;
  });

  private readonly requestedSlots = signal(1);

  constructor(private readonly modal: NgbActiveModal) {
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.formValue.set(this.form.getRawValue());

      // A conflict describes one specific request. The moment any field changes it is
      // about something the user is no longer asking for, so it goes - which also
      // re-enables Reserveren for the new values.
      if (this.conflicts().length > 0) {
        this.conflicts.set([]);
      }

      if (this.errorMessage() !== null) {
        this.errorMessage.set(null);
      }
    });
  }

  /**
   * Called once by the container straight after opening. Setting the starting values
   * here rather than in an effect means they are written exactly once, instead of every
   * time one of the inputs happens to change.
   */
  prefill(options: {
    rooms: readonly Room[];
    bookedBy: string;
    roomId: number | null;
    date: IsoDate;
  }): void {
    this.rooms.set(options.rooms);
    this.bookedBy.set(options.bookedBy);

    this.form.patchValue({
      roomId: options.roomId ?? options.rooms[0]?.id ?? null,
      date: options.date,
    });
  }

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
    if (this.form.invalid || this.invalidTimes()) {
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
