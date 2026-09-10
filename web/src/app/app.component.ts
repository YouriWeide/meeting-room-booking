import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

import { AppHeaderComponent } from './shared/app-header/app-header.component';
import { CurrentUserService } from './shared/current-user';

/**
 * The application shell. Owns the current user so the header can stay presentational.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AppHeaderComponent],
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly currentUser = inject(CurrentUserService);
  private readonly router = inject(Router);

  readonly userName = this.currentUser.name;

  switchUser(): void {
    this.currentUser.forget();
    void this.router.navigate(['/login']);
  }
}
