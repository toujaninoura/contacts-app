import {
  Component,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  OnInit,
  OnDestroy,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil, switchMap } from 'rxjs/operators';

import { ContactService } from '../../../core/services/contact.service';
import { AuthService } from '../../../core/services/auth.service';
import { Contact } from '../../../core/models/contact.model';
import { PagedResponse } from '../../../core/models/api-response.model';

@Component({
  selector: 'app-contact-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact-list.component.html',
  styleUrl: './contact-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactListComponent implements OnInit, OnDestroy {
  private readonly contactService = inject(ContactService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly pageSize = 10;

  contacts: Contact[] = [];
  pagedData: PagedResponse<Contact> | null = null;
  currentPage = 1;
  isLoading = false;

  ngOnInit(): void {
    this.loadContacts();

    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$),
      switchMap(search => {
        this.currentPage = 1;
        this.isLoading = true;
        this.cdr.markForCheck();
        return this.contactService.getAll(this.currentPage, this.pageSize, search);
      })
    ).subscribe({
      next: paged => {
        this.pagedData = paged;
        this.contacts = paged.data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadContacts(): void {
    this.isLoading = true;
    this.contactService.getAll(this.currentPage, this.pageSize, this.searchControl.value).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: paged => {
        this.pagedData = paged;
        this.contacts = paged.data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  deleteContact(id: number): void {
    if (!confirm('Supprimer ce contact ?')) {
      return;
    }

    this.contactService.delete(id).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.loadContacts();
      }
    });
  }

  goToNewContact(): void {
    this.router.navigate(['/contacts/new']);
  }

  goToEdit(id: number): void {
    this.router.navigate(['/contacts', id, 'edit']);
  }

  logout(): void {
    this.authService.logout();
  }

  goToNextPage(): void {
    if (this.pagedData?.hasNext) {
      this.currentPage++;
      this.loadContacts();
    }
  }

  goToPrevPage(): void {
    if (this.pagedData?.hasPrev) {
      this.currentPage--;
      this.loadContacts();
    }
  }

  trackById(_index: number, contact: Contact): number {
    return contact.id;
  }
}
