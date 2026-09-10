import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

/**
 * The application bar: who you are, and a way to become someone else.
 */
@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './app-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppHeaderComponent {
  readonly userName = input.required<string>();

  readonly switchUser = output<void>();
}
