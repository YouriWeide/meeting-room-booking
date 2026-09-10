import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { formatDayLabel, shortTime, today, weekDays } from '../../date-utils';
import type { IsoDate, Occurrence, Room } from '../../models/booking';

/** Which room and day an empty slot belongs to, so booking can be pre-filled. */
export interface SlotSelection {
  readonly roomId: number;
  readonly date: IsoDate;
}

/**
 * Shows, per room, the reservations of the chosen week.
 *
 * Presentational — it is handed rooms and occurrences and reports clicks. It fetches
 * nothing, so the container decides what a week is and where the data comes from.
 */
@Component({
  selector: 'app-week-grid',
  imports: [],
  templateUrl: './week-grid.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WeekGridComponent {
  readonly rooms = input.required<readonly Room[]>();

  readonly occurrences = input.required<readonly Occurrence[]>();

  readonly weekStart = input.required<IsoDate>();

  readonly currentUserName = input.required<string>();

  readonly slotSelected = output<SlotSelection>();

  readonly days = computed(() => weekDays(this.weekStart()));

  readonly todayIso = today();

  /**
   * Every occurrence grouped by the cell it belongs to, keyed by room and day. Built
   * once per change rather than filtering the whole list inside the template for each
   * of the 7 × rooms cells.
   */
  private readonly occurrencesByCell = computed(() => {
    const cells = new Map<string, Occurrence[]>();

    for (const occurrence of this.occurrences()) {
      const key = cellKey(occurrence.roomId, occurrence.date);
      const cellOccurrences = cells.get(key);

      if (cellOccurrences) {
        cellOccurrences.push(occurrence);
      } else {
        cells.set(key, [occurrence]);
      }
    }

    for (const cellOccurrences of cells.values()) {
      cellOccurrences.sort((a, b) => a.start.localeCompare(b.start));
    }

    return cells;
  });

  cell(roomId: number, date: IsoDate): readonly Occurrence[] {
    return this.occurrencesByCell().get(cellKey(roomId, date)) ?? [];
  }

  isMine(occurrence: Occurrence): boolean {
    return occurrence.bookedBy.toLowerCase() === this.currentUserName().toLowerCase();
  }

  protected readonly formatDayLabel = formatDayLabel;

  protected readonly shortTime = shortTime;
}

function cellKey(roomId: number, date: IsoDate): string {
  return `${roomId}|${date}`;
}
