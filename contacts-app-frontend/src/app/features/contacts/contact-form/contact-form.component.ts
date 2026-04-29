import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ContactService } from '../../../core/services/contact.service';
import { CreateContactRequest, UpdateContactRequest } from '../../../core/models/contact.model';

@Component({
  selector: 'app-contact-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './contact-form.component.html',
  styleUrl: './contact-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly contactService = inject(ContactService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  contactForm!: FormGroup;
  isEditMode = false;
  contactId: number | null = null;
  isLoading = false;
  apiError: string | null = null;

  ngOnInit(): void {
    this.buildForm();
    const idParam = this.route.snapshot.params['id'];
    if (idParam) {
      this.isEditMode = true;
      this.contactId = Number(idParam);
      this.loadContact(this.contactId);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): void {
    this.contactForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['']
    });
  }

  private loadContact(id: number): void {
    this.isLoading = true;
    this.contactService.getById(id).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: contact => {
        this.contactForm.patchValue({
          firstName: contact.firstName,
          lastName: contact.lastName,
          email: contact.email,
          phone: contact.phone ?? ''
        });
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onSubmit(): void {
    if (this.contactForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.apiError = null;

    const formValue = this.contactForm.value;

    if (this.isEditMode && this.contactId !== null) {
      const request: UpdateContactRequest = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        email: formValue.email,
        phone: formValue.phone
      };
      this.contactService.update(this.contactId, request).pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: () => {
          this.isLoading = false;
          this.router.navigate(['/contacts']);
        },
        error: err => {
          this.isLoading = false;
          this.apiError = err?.error?.message ?? 'Une erreur est survenue.';
          this.cdr.markForCheck();
        }
      });
    } else {
      const request: CreateContactRequest = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        email: formValue.email,
        phone: formValue.phone
      };
      this.contactService.create(request).pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: () => {
          this.isLoading = false;
          this.router.navigate(['/contacts']);
        },
        error: err => {
          this.isLoading = false;
          this.apiError = err?.error?.message ?? 'Une erreur est survenue.';
          this.cdr.markForCheck();
        }
      });
    }
  }

  get firstNameControl() { return this.contactForm.controls['firstName']; }
  get lastNameControl() { return this.contactForm.controls['lastName']; }
  get emailControl() { return this.contactForm.controls['email']; }
  get phoneControl() { return this.contactForm.controls['phone']; }
}
