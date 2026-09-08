import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { CurrentUserService } from '../../shared/current-user';

/**
 * The week overview.
 */
@Component({
  selector: 'app-schedule',
  imports: [],
  templateUrl: './schedule.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SchedulePage {
  private readonly currentUser = inject(CurrentUserService);

  readonly userName = this.currentUser.name;
}
