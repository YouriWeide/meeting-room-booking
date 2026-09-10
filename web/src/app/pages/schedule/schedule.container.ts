import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { EMPTY, Subject, catchError, switchMap, tap } from 'rxjs';

import { BookingApi, asConflictProblem } from '../../features/schedule/api/booking-api';
import {
  addDays,
  formatWeekLabel,
  startOfWeek,
  today,
} from '../../features/schedule/date-utils';
import { BookingDialogComponent } from '../../features/schedule/components/booking-dialog/booking-dialog.component';
import {
  CancelDialogComponent,
  type CancelScope,
} from '../../features/schedule/components/cancel-dialog/cancel-dialog.component';
import { WeekGridComponent, type SlotSelection } from '../../features/schedule/components/week-grid/week-grid.component';
import type {
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
 * The container: it holds which week is shown, fetches the data, and performs every
 * request. The grid and the two dialogs it opens receive plain data and report what the
 * user did, so none of them knows the API exists.
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

  /** Fires whenever the schedule on screen has to be fetched again. */
  private readonly reload = new Subject<void>();

  constructor() {
    this.reload
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.loadError.set(null);
        }),
        switchMap(() => {
          const from = this.weekStart();

          return this.api.getSchedule(from, addDays(from, DaysInWeek - 1)).pipe(
            catchError((error: unknown) => {
              this.loadError.set(describe(error));
              this.loading.set(false);

              // The week that was on screen stays on screen, with the error above it.
              return EMPTY;
            }),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((occurrences) => {
        this.occurrences.set(occurrences);
        this.loading.set(false);
      });

    this.loadRooms();
    this.loadSchedule();
  }

  /** The week on screen may already be in the past; the API refuses past dates. */
  private firstBookableDate(): IsoDate {
    const start = this.weekStart();
    return start < today() ? today() : start;
  }

  readonly weekLabel = computed(() => formatWeekLabel(this.weekStart()));

  showWeek(offsetInWeeks: number): void {
    this.weekStart.update((current) => addDays(current, offsetInWeeks * DaysInWeek));
    this.loadSchedule();
  }

  showThisWeek(): void {
    this.weekStart.set(startOfWeek(today()));
    this.loadSchedule();
  }

  openBooking(slot?: SlotSelection): void {
    const dialog = this.modal.open(BookingDialogComponent, { size: 'lg' });
    const form = dialog.componentInstance as BookingDialogComponent;

    form.prefill({
      rooms: this.rooms(),
      bookedBy: this.userName() ?? '',
      roomId: slot?.roomId ?? null,
      date: slot?.date ?? this.firstBookableDate(),
    });

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
        this.loadSchedule();
      },
      error: (error: unknown) => {
        form.busy.set(false);

        const conflict = asConflictProblem(error);

        if (conflict) {
          form.conflicts.set(conflict.conflicts);
          return;
        }

        form.errorMessage.set(describe(error));
      },
    });
  }

  /**
   * Cancel one occurrence or the whole series. Which of the two is the user's choice,
   * so the dialog asks and this decides nothing on their behalf.
   */
  cancelBooking(occurrence: Occurrence): void {
    const dialog = this.modal.open(CancelDialogComponent);
    const confirmation = dialog.componentInstance as CancelDialogComponent;

    confirmation.occurrence.set(occurrence);
    confirmation.roomName.set(this.roomName(occurrence.roomId));

    // The promise rejects when the dialog is dismissed.
    dialog.result.then(
      (scope: CancelScope) => this.performCancel(occurrence, scope),
      () => undefined,
    );
  }

  private performCancel(occurrence: Occurrence, scope: CancelScope): void {
    const bookedBy = this.userName() ?? '';

    const request =
      scope === 'series'
        ? this.api.cancelSeries(occurrence.reservationId, bookedBy)
        : this.api.cancelOccurrence(occurrence.reservationId, occurrence.date, bookedBy);

    request.subscribe({
      next: () => this.loadSchedule(),
      error: (error: unknown) => this.loadError.set(describe(error)),
    });
  }

  private roomName(roomId: number): string {
    return this.rooms().find((room) => room.id === roomId)?.name ?? '';
  }

  /**
   * Fetched once. The rooms are seeded and do not change while the app is open.
   */
  private loadRooms(): void {
    this.api.getRooms().subscribe({
      next: (rooms) => this.rooms.set(rooms),
      error: (error: unknown) => this.loadError.set(describe(error)),
    });
  }

  private loadSchedule(): void {
    this.reload.next();
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
