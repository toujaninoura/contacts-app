import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';

import { ContactFormComponent } from './contact-form.component';
import { ContactService } from '../../../core/services/contact.service';
import { Contact } from '../../../core/models/contact.model';

const mockContact: Contact = {
  id: 1,
  firstName: 'Alice',
  lastName: 'Dupont',
  email: 'alice@example.com',
  phone: '0600000000',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z'
};

function buildActivatedRoute(params: Record<string, string> = {}): Partial<ActivatedRoute> {
  return {
    snapshot: { params } as ActivatedRoute['snapshot']
  };
}

describe('ContactFormComponent', () => {
  let component: ContactFormComponent;
  let fixture: ComponentFixture<ContactFormComponent>;
  let contactServiceSpy: jasmine.SpyObj<ContactService>;
  let router: Router;

  function createComponent(params: Record<string, string> = {}): void {
    TestBed.configureTestingModule({
      imports: [ContactFormComponent, ReactiveFormsModule, RouterTestingModule],
      providers: [
        { provide: ContactService, useValue: contactServiceSpy },
        { provide: ActivatedRoute, useValue: buildActivatedRoute(params) }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));

    fixture = TestBed.createComponent(ContactFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(() => {
    contactServiceSpy = jasmine.createSpyObj('ContactService', ['getById', 'create', 'update']);

    contactServiceSpy.getById.and.returnValue(of(mockContact));
    contactServiceSpy.create.and.returnValue(of(mockContact));
    contactServiceSpy.update.and.returnValue(of(mockContact));
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('should create the component', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should be in create mode when no id in route', () => {
    createComponent();
    expect(component.isEditMode).toBeFalse();
    expect(component.contactId).toBeNull();
  });

  it('should be in edit mode when id in route params', () => {
    createComponent({ id: '1' });
    expect(component.isEditMode).toBeTrue();
    expect(component.contactId).toBe(1);
  });

  it('should load contact data in edit mode', () => {
    createComponent({ id: '1' });
    expect(contactServiceSpy.getById).toHaveBeenCalledWith(1);
    expect(component.contactForm.value.firstName).toBe('Alice');
    expect(component.contactForm.value.lastName).toBe('Dupont');
    expect(component.contactForm.value.email).toBe('alice@example.com');
    expect(component.contactForm.value.phone).toBe('0600000000');
  });

  it('should show validation error when firstName is empty', () => {
    createComponent();
    component.contactForm.controls['firstName'].markAsTouched();
    component.contactForm.controls['firstName'].setValue('');
    fixture.detectChanges();

    const firstNameControl = component.contactForm.controls['firstName'];
    expect(firstNameControl.invalid).toBeTrue();
    expect(firstNameControl.errors?.['required']).toBeTrue();
  });

  it('should show validation error when email is invalid', () => {
    createComponent();
    component.contactForm.controls['email'].markAsTouched();
    component.contactForm.controls['email'].setValue('not-an-email');
    fixture.detectChanges();

    const emailControl = component.contactForm.controls['email'];
    expect(emailControl.invalid).toBeTrue();
    expect(emailControl.errors?.['email']).toBeTrue();
  });

  it('should call contactService.create when form is valid in create mode', () => {
    createComponent();
    component.contactForm.setValue({
      firstName: 'Bob',
      lastName: 'Martin',
      email: 'bob@example.com',
      phone: ''
    });

    component.onSubmit();

    expect(contactServiceSpy.create).toHaveBeenCalledWith({
      firstName: 'Bob',
      lastName: 'Martin',
      email: 'bob@example.com',
      phone: ''
    });
  });

  it('should call contactService.update when form is valid in edit mode', () => {
    createComponent({ id: '1' });
    component.contactForm.setValue({
      firstName: 'Alice',
      lastName: 'Dupont',
      email: 'alice@example.com',
      phone: '0600000000'
    });

    component.onSubmit();

    expect(contactServiceSpy.update).toHaveBeenCalledWith(1, {
      firstName: 'Alice',
      lastName: 'Dupont',
      email: 'alice@example.com',
      phone: '0600000000'
    });
  });

  it('should navigate to /contacts on success', () => {
    createComponent();
    component.contactForm.setValue({
      firstName: 'Bob',
      lastName: 'Martin',
      email: 'bob@example.com',
      phone: ''
    });

    component.onSubmit();

    expect(router.navigate).toHaveBeenCalledWith(['/contacts']);
  });

  it('should show API error message on failure', () => {
    contactServiceSpy.create.and.returnValue(
      throwError(() => ({ error: { message: 'Email déjà utilisé' } }))
    );
    createComponent();
    component.contactForm.setValue({
      firstName: 'Bob',
      lastName: 'Martin',
      email: 'bob@example.com',
      phone: ''
    });

    component.onSubmit();

    expect(component.apiError).toBe('Email déjà utilisé');
  });

  it('should disable save button when form is invalid', () => {
    createComponent();
    component.contactForm.controls['firstName'].setValue('');
    component.contactForm.controls['lastName'].setValue('');
    component.contactForm.controls['email'].setValue('');
    fixture.detectChanges();

    expect(component.contactForm.invalid).toBeTrue();
  });
});
