import type { IsoDate } from './models/booking';

/**
 * Calendar arithmetic on 'yyyy-MM-dd' strings.
 */

const DaysInWeek = 7;

function toIsoDate(date: Date): IsoDate {
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

/** Local midnight on the given day, so no timezone conversion can happen. */
function fromIsoDate(value: IsoDate): Date {
  const [year, month, day] = value.split('-').map(Number);
  return new Date(year, month - 1, day);
}

export function today(): IsoDate {
  return toIsoDate(new Date());
}

export function addDays(value: IsoDate, days: number): IsoDate {
  const date = fromIsoDate(value);
  date.setDate(date.getDate() + days);
  return toIsoDate(date);
}

/** The Monday of the week containing this date. */
export function startOfWeek(value: IsoDate): IsoDate {
  const date = fromIsoDate(value);
  const dayOfWeek = date.getDay();
  const offset = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
  return addDays(value, offset);
}

/** The seven dates of the week starting at `monday`. */
export function weekDays(monday: IsoDate): IsoDate[] {
  return Array.from({ length: DaysInWeek }, (_, index) => addDays(monday, index));
}

export function formatDayLabel(value: IsoDate): string {
  return fromIsoDate(value).toLocaleDateString('nl-NL', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  });
}

export function formatWeekLabel(monday: IsoDate): string {
  const sunday = addDays(monday, DaysInWeek - 1);
  const start = fromIsoDate(monday).toLocaleDateString('nl-NL', { day: 'numeric', month: 'long' });
  const end = fromIsoDate(sunday).toLocaleDateString('nl-NL', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });
  return `${start} – ${end}`;
}

export function shortTime(value: string): string {
  return value.slice(0, 5);
}
