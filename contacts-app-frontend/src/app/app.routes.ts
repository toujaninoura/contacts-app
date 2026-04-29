import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'contacts', pathMatch: 'full' },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'contacts',
    canActivate: [authGuard],
    loadChildren: () => import('./features/contacts/contacts.routes').then(m => m.CONTACTS_ROUTES)
  },
  { path: '**', redirectTo: 'contacts' }
];
