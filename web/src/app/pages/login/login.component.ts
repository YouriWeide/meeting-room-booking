import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { CurrentUserService } from '../../shared/current-user';

/**
 * Asks who you are. Container: it owns the service and the navigation.
 */
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly currentUser = inject(CurrentUserService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
  });

  get name(): FormControl<string> {
    return this.form.controls.name;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.currentUser.setName(this.name.value);

    // Back to wherever the guard interrupted, or the schedule if they came here directly.
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/schedule';
    void this.router.navigateByUrl(returnUrl);
  }
}
