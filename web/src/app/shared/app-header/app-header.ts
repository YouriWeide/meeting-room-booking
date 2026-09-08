import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

/**
 * The application bar: who you are, and a way to become someone else.
 */
@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './app-header.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppHeader {
  readonly userName = input.required<string>();

  readonly switchUser = output<void>();
}
