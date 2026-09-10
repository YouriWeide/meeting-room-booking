import { Routes } from '@angular/router';

import { requireUserGuard } from './shared/require-user-guard';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Wie ben je? — Vergaderruimtes',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'schedule',
    title: 'Overzicht — Vergaderruimtes',
    canActivate: [requireUserGuard],
    loadComponent: () => import('./pages/schedule/schedule.container').then((m) => m.ScheduleContainerComponent),
  },
  { path: '', pathMatch: 'full', redirectTo: 'schedule' },
  { path: '**', redirectTo: 'schedule' },
];
