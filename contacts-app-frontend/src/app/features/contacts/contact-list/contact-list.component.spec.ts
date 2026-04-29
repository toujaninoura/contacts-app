import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { ContactListComponent } from './contact-list.component';
import { ContactService } from '../../../core/services/contact.service';
import { AuthService } from '../../../core/services/auth.service';
import { PagedResponse } from '../../../core/models/api-response.model';
import { Contact } from '../../../core/models/contact.model';

const mockContact: Contact = {
  id: 1,
  firstName: 'Alice',
  lastName: 'Martin',
  email: 'alice@example.com',
  phone: '0600000001',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z'
};

const mockPagedResponse: PagedResponse<Contact> = {
  data: [mockContact],
  page: 1,
  pageSize: 10,
  totalCount: 1,
  totalPages: 1,
  hasNext: false,
  hasPrev: false
};

const emptyPagedResponse: PagedResponse<Contact> = {
  data: [],
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
  hasNext: false,
  hasPrev: false
};

describe('ContactListComponent', () => {
  let component: ContactListComponent;
  let fixture: ComponentFixture<ContactListComponent>;
  let contactServiceSpy: jasmine.SpyObj<ContactService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    contactServiceSpy = jasmine.createSpyObj('ContactService', ['getAll', 'delete']);
    authServiceSpy = jasmine.createSpyObj('AuthService', ['logout']);

    contactServiceSpy.getAll.and.returnValue(of(mockPagedResponse));
    contactServiceSpy.delete.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [ContactListComponent],
      providers: [
        { provide: ContactService, useValue: contactServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ContactListComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load contacts on init', () => {
    expect(contactServiceSpy.getAll).toHaveBeenCalledWith(1, 10, '');
    expect(component.contacts.length).toBe(1);
    expect(component.contacts[0].firstName).toBe('Alice');
  });

  it('should filter contacts when search input changes', fakeAsync(() => {
    component.searchControl.setValue('Alice');
    tick(300);
    fixture.detectChanges();

    expect(contactServiceSpy.getAll).toHaveBeenCalledWith(1, 10, 'Alice');
  }));

  it('should delete contact after confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    contactServiceSpy.getAll.and.returnValue(of(emptyPagedResponse));

    component.deleteContact(1);

    expect(contactServiceSpy.delete).toHaveBeenCalledWith(1);
  });

  it('should not delete contact when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);

    component.deleteContact(1);

    expect(contactServiceSpy.delete).not.toHaveBeenCalled();
  });

  it('should navigate to new contact page', () => {
    const navigateSpy = spyOn(router, 'navigate');

    component.goToNewContact();

    expect(navigateSpy).toHaveBeenCalledWith(['/contacts/new']);
  });

  it('should call logout when deconnexion button clicked', () => {
    component.logout();

    expect(authServiceSpy.logout).toHaveBeenCalled();
  });

  it('should show empty message when no contacts', () => {
    contactServiceSpy.getAll.and.returnValue(of(emptyPagedResponse));
    component.loadContacts();
    fixture.detectChanges();

    expect(component.contacts.length).toBe(0);
    const compiled = fixture.nativeElement as HTMLElement;
    const emptyMsg = compiled.querySelector('[data-testid="empty-message"]');
    expect(emptyMsg).toBeTruthy();
    expect(emptyMsg?.textContent).toContain('Aucun contact trouvé');
  });

  it('should track contacts by id', () => {
    const result = component.trackById(0, mockContact);
    expect(result).toBe(1);
  });

  it('should go to next page when hasNext is true', () => {
    component.pagedData = { ...mockPagedResponse, hasNext: true, totalPages: 2 };
    component.goToNextPage();
    expect(contactServiceSpy.getAll).toHaveBeenCalledWith(2, 10, '');
  });

  it('should go to previous page when hasPrev is true', () => {
    component.pagedData = { ...mockPagedResponse, page: 2, hasPrev: true, totalPages: 2 };
    component.currentPage = 2;
    component.goToPrevPage();
    expect(contactServiceSpy.getAll).toHaveBeenCalledWith(1, 10, '');
  });
});
