import { Routes } from '@angular/router';

import { ContactListComponent } from './contact-list/contact-list.component';
import { ContactFormComponent } from './contact-form/contact-form.component';

export const CONTACTS_ROUTES: Routes = [
  { path: '', component: ContactListComponent },
  { path: 'new', component: ContactFormComponent },
  { path: ':id/edit', component: ContactFormComponent }
];
