import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { forkJoin } from 'rxjs';

import { BookingApi, asConflictProblem } from '../../features/schedule/api/booking-api';
import {
  addDays,
  formatWeekLabel,
  startOfWeek,
  today,
} from '../../features/schedule/date-utils';
import { BookingDialogComponent } from '../../features/schedule/components/booking-dialog/booking-dialog.component';
import { WeekGridComponent, type SlotSelection } from '../../features/schedule/components/week-grid/week-grid.component';
import type {
  Conflict,
  CreateReservationRequest,
  IsoDate,
  Occurrence,
  Room,
} from '../../features/schedule/models/booking';
import { CurrentUserService } from '../../shared/current-user';

const DaysInWeek = 7;

/**
 * The week overview, and where booking starts.
 *
 * The container: it holds which week is shown, fetches rooms and occurrences, and runs
 * the booking request. The grid and the dialog below it receive plain data and report
 * intent, so neither of them knows the API exists.
 */
@Component({
  selector: 'app-schedule',
  imports: [WeekGridComponent],
  templateUrl: './schedule.container.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScheduleContainerComponent {
  private readonly api = inject(BookingApi);
  private readonly modal = inject(NgbModal);
  private readonly currentUser = inject(CurrentUserService);

  readonly userName = this.currentUser.name;

  readonly weekStart = signal<IsoDate>(startOfWeek(today()));
  readonly rooms = signal<readonly Room[]>([]);
  readonly occurrences = signal<readonly Occurrence[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  weekLabel(): string {
    return formatWeekLabel(this.weekStart());
  }

  showWeek(offsetInWeeks: number): void {
    this.weekStart.update((current) => addDays(current, offsetInWeeks * DaysInWeek));
    this.load();
  }

  showThisWeek(): void {
    this.weekStart.set(startOfWeek(today()));
    this.load();
  }

  openBooking(slot?: SlotSelection): void {
    const dialog = this.modal.open(BookingDialogComponent, { size: 'lg' });
    const form = dialog.componentInstance as BookingDialogComponent;

    form.rooms.set(this.rooms());
    form.bookedBy.set(this.userName() ?? '');
    form.defaultRoomId.set(slot?.roomId ?? null);
    // The week on screen may have started before today; booking the past is refused
    // by the API, so the form should never open already invalid.
    const start = this.weekStart();
    form.defaultDate.set(slot?.date ?? (start < today() ? today() : start));

    form.submitted.subscribe((request) => this.book(dialog, form, request));
  }

  private book(
    dialog: { close: () => void },
    form: BookingDialogComponent,
    request: CreateReservationRequest,
  ): void {
    form.busy.set(true);
    form.errorMessage.set(null);

    this.api.createReservation(request).subscribe({
      next: () => {
        dialog.close();
        this.load();
      },
      error: (error: unknown) => {
        form.busy.set(false);

        const conflict = asConflictProblem(error);

        if (conflict) {
          form.conflicts.set(conflict.conflicts as readonly Conflict[]);
          return;
        }

        form.errorMessage.set(describe(error));
      },
    });
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const from = this.weekStart();
    const to = addDays(from, DaysInWeek - 1);

    forkJoin({
      rooms: this.api.getRooms(),
      occurrences: this.api.getSchedule(from, to),
    }).subscribe({
      next: ({ rooms, occurrences }) => {
        this.rooms.set(rooms);
        this.occurrences.set(occurrences);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loadError.set(describe(error));
        this.loading.set(false);
      },
    });
  }
}

function describe(error: unknown): string {
  const body = (error as {
    error?: { detail?: string; title?: string; errors?: Record<string, string[]> };
  })?.error;

  // Validation problems put the useful part in `errors`; the title is only ever
  // "One or more validation errors occurred", which tells the user nothing.
  const fieldErrors = Object.values(body?.errors ?? {}).flat();

  if (fieldErrors.length) {
    return fieldErrors.join(' ');
  }

  return body?.detail ?? body?.title ?? 'Er ging iets mis. Probeer het opnieuw.';
}
