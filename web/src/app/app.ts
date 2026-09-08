import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

import { AppHeader } from './shared/app-header/app-header';
import { CurrentUserService } from './shared/current-user';

/**
 * The application shell. Owns the current user so the header can stay presentational.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AppHeader],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly currentUser = inject(CurrentUserService);
  private readonly router = inject(Router);

  readonly userName = this.currentUser.name;

  switchUser(): void {
    this.currentUser.forget();
    void this.router.navigate(['/login']);
  }
}
