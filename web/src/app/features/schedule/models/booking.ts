export type IsoDate = string;

export type IsoTime = string;

export interface Room {
  readonly id: number;
  readonly name: string;
  readonly capacity: number;
}

export interface Occurrence {
  readonly reservationId: number;
  readonly roomId: number;
  readonly date: IsoDate;
  readonly start: IsoTime;
  readonly end: IsoTime;
  readonly bookedBy: string;
  readonly seriesOccurrences: number;
}

export interface CreateReservationRequest {
  readonly roomId: number;
  readonly bookedBy: string;
  readonly date: IsoDate;
  readonly startTime: IsoTime;
  readonly endTime: IsoTime;
  readonly weeks: number;
  readonly skipConflicts?: boolean;
}

export interface Reservation {
  readonly id: number;
  readonly roomId: number;
  readonly bookedBy: string;
  readonly firstDate: IsoDate;
  readonly lastDate: IsoDate;
  readonly startTime: IsoTime;
  readonly endTime: IsoTime;
  readonly weeks: number;
  readonly occurrences: readonly Occurrence[];
  readonly skippedDates: readonly IsoDate[];
}

export interface Conflict {
  readonly date: IsoDate;
  readonly start: IsoTime;
  readonly end: IsoTime;
  readonly conflictsWith: {
    readonly reservationId: number;
    readonly bookedBy: string;
    readonly start: IsoTime;
    readonly end: IsoTime;
  };
}

export interface ConflictProblem {
  readonly title: string;
  readonly status: number;
  readonly detail?: string;
  readonly conflicts: readonly Conflict[];
}
