import { Injectable, computed, signal } from '@angular/core';

const StorageKey = 'roombooking.name';

/**
 * Who is using the app, held as a name and nothing else.
 */
@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private readonly currentName = signal<string | null>(readStoredName());

  /** The current name, or null when nobody has said who they are yet. */
  readonly name = this.currentName.asReadonly();

  readonly isKnown = computed(() => this.currentName() !== null);

  setName(name: string): void {
    const trimmed = name.trim();

    if (trimmed.length === 0) {
      return;
    }

    this.currentName.set(trimmed);
    writeStoredName(trimmed);
  }

  /** Forgets the name, so the next visit asks again. Used by "wissel gebruiker". */
  forget(): void {
    this.currentName.set(null);
    writeStoredName(null);
  }
}

function readStoredName(): string | null {
  try {
    return localStorage.getItem(StorageKey);
  } catch {
    return null;
  }
}

function writeStoredName(name: string | null): void {
  try {
    if (name === null) {
      localStorage.removeItem(StorageKey);
    } else {
      localStorage.setItem(StorageKey, name);
    }
  } catch {
    // Ignored on purpose: the signal is still correct for this session.
  }
}
