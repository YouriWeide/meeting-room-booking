import { Routes } from '@angular/router';

import { requireUserGuard } from './shared/require-user-guard';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Wie ben je? — Vergaderruimtes',
    loadComponent: () => import('./pages/login/login').then((m) => m.LoginPage),
  },
  {
    path: 'schedule',
    title: 'Overzicht — Vergaderruimtes',
    canActivate: [requireUserGuard],
    loadComponent: () => import('./pages/schedule/schedule').then((m) => m.SchedulePage),
  },
  { path: '', pathMatch: 'full', redirectTo: 'schedule' },
  { path: '**', redirectTo: 'schedule' },
];
