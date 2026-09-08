import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import type {
  ConflictProblem,
  CreateReservationRequest,
  IsoDate,
  Occurrence,
  Reservation,
  Room,
} from '../models/booking';


@Injectable({ providedIn: 'root' })
export class BookingApi {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api';

  getRooms(): Observable<Room[]> {
    return this.http.get<Room[]>(`${this.baseUrl}/rooms`);
  }

  getSchedule(from: IsoDate, to: IsoDate, roomId?: number): Observable<Occurrence[]> {
    let params = new HttpParams().set('from', from).set('to', to);

    if (roomId !== undefined) {
      params = params.set('roomId', roomId);
    }

    return this.http.get<Occurrence[]>(`${this.baseUrl}/schedule`, { params });
  }

  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.baseUrl}/reservations`, request);
  }

  cancelSeries(reservationId: number, bookedBy: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/reservations/${reservationId}`, {
      params: new HttpParams().set('bookedBy', bookedBy),
    });
  }

  cancelOccurrence(
    reservationId: number,
    occurrenceDate: IsoDate,
    bookedBy: string,
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/reservations/${reservationId}/occurrences/${occurrenceDate}`,
      { params: new HttpParams().set('bookedBy', bookedBy) },
    );
  }
}

/**
 * Narrows a failed request to the conflict response, so callers can offer "book the
 * remaining weeks" instead of only being able to show a message.
 */
export function asConflictProblem(error: unknown): ConflictProblem | null {
  if (error instanceof HttpErrorResponse && error.status === 409 && Array.isArray(error.error?.conflicts)) {
    return error.error as ConflictProblem;
  }

  return null;
}
